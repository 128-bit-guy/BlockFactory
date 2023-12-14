using System.Net;
using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Server;
using ENet.Managed;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Server)]
public class ServerNetworkHandler : MultiPlayerNetworkHandler
{
    private readonly Dictionary<ENetPeer, ServerPlayerEntity> _players;

    public ServerNetworkHandler(int port) : base(LogicalSide.Server,
        new ENetHost(new IPEndPoint(IPAddress.Loopback, port), 16, 1))
    {
        _players = new Dictionary<ENetPeer, ServerPlayerEntity>();
    }

    protected override void OnPeerConnected(ENetPeer peer)
    {
        Console.WriteLine($"Client with address {peer.GetRemoteEndPoint()} connected");
        var player = new ServerPlayerEntity(peer);
        BlockFactoryServer.LogicProcessor.AddPlayer(player);
        player.SetWorld(BlockFactoryServer.LogicProcessor.GetWorld());
        _players.Add(peer, player);
    }

    protected override void OnPeerDisconnected(ENetPeer peer)
    {
        Console.WriteLine($"Client with address {peer.GetRemoteEndPoint()} disconnected");
        _players.Remove(peer, out var player);
        player!.SetWorld(null);
        BlockFactoryServer.LogicProcessor.RemovePlayer(player);
    }

    protected override void OnPacketReceived(IPacket packet, ENetPeer peer)
    {
        var player = _players[peer];
        try
        {
            packet.Handle(player);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
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