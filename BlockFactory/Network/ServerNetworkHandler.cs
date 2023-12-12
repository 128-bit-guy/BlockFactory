using System.Net;
using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using BlockFactory.Server;
using ENet.Managed;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Server)]
public class ServerNetworkHandler : MultiPlayerNetworkHandler
{
    private readonly Dictionary<ENetPeer, ServerPlayerEntity> _players;
    public ServerNetworkHandler(int port) : base(new ENetHost(new IPEndPoint(IPAddress.Loopback, port), 16, 1))
    {
        _players = new Dictionary<ENetPeer, ServerPlayerEntity>();
    }

    public override bool ShouldStop()
    {
        return false;
    }

    public override void SendPacket<T>(PlayerEntity? player, T packet)
    {
        var arr = SerializePacket(packet);
        var serverPlayer = (ServerPlayerEntity)player!;
        if(!_players.ContainsKey(serverPlayer.Peer))  return;
        try
        {
            serverPlayer.Peer.Send(0, arr, NetworkRegistry.GetPacketFlags<T>());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    protected override void Connect(ENetEvent evt)
    {
        Console.WriteLine($"Client with address {evt.Peer.GetRemoteEndPoint()} connected");
        var player = new ServerPlayerEntity(evt.Peer);
        BlockFactoryServer.LogicProcessor.AddPlayer(player);
        player.SetWorld(BlockFactoryServer.LogicProcessor.GetWorld());
        _players.Add(evt.Peer, player);
    }

    protected override void Disconnect(ENetEvent evt)
    {
        Console.WriteLine($"Client with address {evt.Peer.GetRemoteEndPoint()} disconnected");
        _players.Remove(evt.Peer, out var player);
        player!.SetWorld(null);
        BlockFactoryServer.LogicProcessor.RemovePlayer(player);
    }

    protected override void Receive(ENetEvent evt)
    {
        var p = DeserializePacket(evt.Packet.Data.ToArray());
        var player = _players[evt.Peer];
        if (p.SupportsLogicalSide(LogicalSide.Server))
        {
            p.Handle(player);
        }
    }
}