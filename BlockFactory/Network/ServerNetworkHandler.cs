using System.Collections;
using System.Net;
using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Network.Packet_;
using BlockFactory.Server;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Server)]
public class ServerNetworkHandler : MultiPlayerNetworkHandler
{
    private readonly Dictionary<ENetPeer, ServerPeerState> _players;

    public ServerNetworkHandler(int port) : base(LogicalSide.Server,
        new ENetHost(new IPEndPoint(IPAddress.Loopback, port), 16, 1))
    {
        _players = new Dictionary<ENetPeer, ServerPeerState>();
    }

    protected override void OnPeerConnected(ENetPeer peer)
    {
        Console.WriteLine($"Client with address {peer.GetRemoteEndPoint()} connected");
        var peerState = new ServerPeerState();
        peerState.PreGameEnumerator = GetPreGameEnumerator(peer, peerState);
        _players.Add(peer, peerState);
        peerState.PreGameEnumerator.MoveNext();
    }

    private ServerPlayerEntity CreatePlayer(ENetPeer peer)
    {
        var player = new ServerPlayerEntity(peer);
        player.HeadRotation = new Vector2D<float>((float)Random.Shared.NextDouble() * 2 * MathF.PI,
            (float)Random.Shared.NextDouble() * MathF.PI - MathF.PI / 2);
        BlockFactoryServer.LogicProcessor.AddPlayer(player);
        player.SetWorld(BlockFactoryServer.LogicProcessor.GetWorld());
        SendPacket(player, new PlayerDataPacket(player));
        return player;
    }

    protected override void OnPeerDisconnected(ENetPeer peer)
    {
        Console.WriteLine($"Client with address {peer.GetRemoteEndPoint()} disconnected");
        _players.Remove(peer, out var peerState);
        if (peerState!.Player != null)
        {
            peerState.Player!.SetWorld(null);
            BlockFactoryServer.LogicProcessor.RemovePlayer(peerState.Player);
        }
    }

    protected override void OnPacketReceived(IPacket packet, ENetPeer peer)
    {
        var peerState = _players[peer];
        if (peerState.Player == null)
        {
            peerState.PreGameQueue.Enqueue(packet);
            peerState.PreGameEnumerator.MoveNext();
        }
        else
        {
            if (packet is IInGamePacket p)
            {
                try
                {
                    p.Handle(peerState.Player);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }

    private IEnumerator GetPreGameEnumerator(ENetPeer peer, ServerPeerState peerState)
    {
        foreach (var _ in peerState.WaitForPreGamePacket<CredentialsPacket>()) yield return null;
        var p = (CredentialsPacket)peerState.PreGameQueue.Dequeue();
        Console.WriteLine($"Player {p.Credentials.Name} logged in with password {p.Credentials.Password}");
        EnqueueSendPacketInternal(new RegistryMappingPacket(BlockFactoryServer.Mapping), peer);
        var player = CreatePlayer(peer);
        peerState.Player = player;
        SendPacket(player, new PlayerDataPacket(player));
        yield break;
    }

    public override bool ShouldStop()
    {
        return false;
    }

    public override void SendPacket<T>(PlayerEntity? player, T packet)
    {
        var serverPlayer = (ServerPlayerEntity)player!;
        EnqueueSendPacketInternal(packet, serverPlayer.Peer);
    }
}