using System.Net;
using ENet.Managed;

namespace BlockFactory.Network;

public class ServerNetworkHandler : MultiPlayerNetworkHandler
{
    public ServerNetworkHandler(int port) : base(new ENetHost(new IPEndPoint(IPAddress.Loopback, port), 16, 1))
    {
    }

    public override bool ShouldStop()
    {
        return false;
    }

    protected override void ProcessEvent(ENetEvent evt)
    {
        
    }
}