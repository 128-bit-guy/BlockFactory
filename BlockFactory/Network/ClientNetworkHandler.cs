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
        var arr = SerializePacket(packet);
        try
        {
            _peer.Send(0, arr, NetworkRegistry.GetPacketFlags<T>());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
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
        var p = DeserializePacket(evt.Packet.Data.ToArray());
        if (p.SupportsLogicalSide(LogicalSide.Client))
        {
            p.Handle(null);
        }
    }
}