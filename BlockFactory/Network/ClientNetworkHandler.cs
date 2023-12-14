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

    public ClientNetworkHandler(IPEndPoint remote) : base(LogicalSide.Client,
        new ENetHost(null, 1, 1))
    {
        _peer = Host.Connect(remote, 1, 0);
        _connected = true;
    }

    protected override void OnPeerConnected(ENetPeer peer)
    {
        if (peer != _peer)
        {
            peer.Disconnect(0);
        }
    }

    protected override void OnPeerDisconnected(ENetPeer peer)
    {
        if (peer == _peer)
        {
            _connected = false;
        }
    }

    protected override void OnPacketReceived(IPacket packet, ENetPeer peer)
    {
        try
        {
            packet.Handle(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public override bool ShouldStop()
    {
        return !_connected;
    }

    public override void SendPacket<T>(PlayerEntity? player, T packet)
    {
        EnqueueSendPacketInternal(packet, _peer);
    }
}