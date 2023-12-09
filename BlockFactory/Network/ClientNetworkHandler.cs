using System.Net;
using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Client)]
public class ClientNetworkHandler : MultiPlayerNetworkHandler
{
    private readonly ENetPeer _peer;
    private bool _connected;
    public ClientNetworkHandler(IPEndPoint remote) : base(new ENetHost(null, 1, 1))
    {
        _peer = Host.Connect(remote, 1, 0);
        _connected = true;
    }

    public override bool ShouldStop()
    {
        return !_connected;
    }

    public override void SendPacket<T>(PlayerEntity? player, T packet)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write7BitEncodedInt(NetworkRegistry.GetPacketTypeId<T>());
        packet.SerializeBinary(writer, SerializationReason.NetworkUpdate);
        _peer.Send(0, stream.ToArray(), NetworkRegistry.GetPacketFlags<T>());
    }

    protected override void Connect(ENetEvent evt)
    {
        if (evt.Peer != _peer)
        {
            evt.Peer.Disconnect(0);
        }
    }

    protected override void Disconnect(ENetEvent evt)
    {
        if (evt.Peer == _peer)
        {
            _connected = false;
        }
    }

    protected override void Receive(ENetEvent evt)
    {
        using var stream = new MemoryStream(evt.Packet.Data.ToArray());
        using var reader = new BinaryReader(stream);
        var id = reader.Read7BitEncodedInt();
        var p = NetworkRegistry.CreatePacket(id);
        p.DeserializeBinary(reader, SerializationReason.NetworkUpdate);
        if (p.SupportsLogicalSide(LogicalSide.Client))
        {
            p.Handle(null);
        }
    }
}