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
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write7BitEncodedInt(NetworkRegistry.GetPacketTypeId<T>());
        packet.SerializeBinary(writer, SerializationReason.NetworkUpdate);
        var serverPlayer = (ServerPlayerEntity)player!;
        serverPlayer.Peer.Send(0, stream.ToArray(), NetworkRegistry.GetPacketFlags<T>());
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
        using var stream = new MemoryStream(evt.Packet.Data.ToArray());
        using var reader = new BinaryReader(stream);
        var id = reader.Read7BitEncodedInt();
        var p = NetworkRegistry.CreatePacket(id);
        p.DeserializeBinary(reader, SerializationReason.NetworkUpdate);
        var player = _players[evt.Peer];
        if (p.SupportsLogicalSide(LogicalSide.Server))
        {
            p.Handle(player);
        }
    }
}