using System.Net;
using ENet.Managed;

namespace BlockFactory.Network;

public class ClientNetworkHandler : MultiPlayerNetworkHandler
{
    private ENetPeer _peer;
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

    protected override void ProcessEvent(ENetEvent evt)
    {
        if (evt.Type == ENetEventType.Disconnect)
        {
            _connected = false;
            return;
        }
        
        if(evt.Type != ENetEventType.Receive) return;
        
        evt.Packet.Destroy();
    }
}