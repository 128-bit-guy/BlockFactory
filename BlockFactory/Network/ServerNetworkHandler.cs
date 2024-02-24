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

    private ServerPlayerEntity LoadOrCreatePlayer(string name, ENetPeer peer)
    {
        ServerPlayerEntity player;
        if (BlockFactoryServer.LogicProcessor.PlayerData.Players.TryGetValue(name, out var p))
        {
            player = (ServerPlayerEntity)p;
        }
        else
        {
            player = new ServerPlayerEntity();
            player.HeadRotation = new Vector2D<float>((float)Random.Shared.NextDouble() * 2 * MathF.PI,
                (float)Random.Shared.NextDouble() * MathF.PI - MathF.PI / 2);
            BlockFactoryServer.LogicProcessor.PlayerData.Players.Add(name, player);
        }

        player.Peer = peer;
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
            peerState.Player!.Peer = default;
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
        if (!BlockFactoryServer.LogicProcessor.PlayerData.AttemptLogin(p.Credentials))
        {
            Console.WriteLine($"Player {p.Credentials.Name} attempted to log in with incorrect password");
            peer.Disconnect(0);
            yield break;
        }

        if (BlockFactoryServer.LogicProcessor.PlayerData.Players.TryGetValue(p.Credentials.Name, out var pl))
        {
            if (pl.World != null)
            {
                
                Console.WriteLine($"Player {p.Credentials.Name} attempted to log in from two clients at once");
                peer.Disconnect(0);
                yield break;
            }
        }
        Console.WriteLine($"Player {p.Credentials.Name} logged in");
        EnqueueSendPacketInternal(new RegistryMappingPacket(BlockFactoryServer.Mapping), peer);
        var player = LoadOrCreatePlayer(p.Credentials.Name, peer);
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