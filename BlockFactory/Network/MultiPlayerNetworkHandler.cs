using BlockFactory.Entity_;
using ENet.Managed;

namespace BlockFactory.Network;

public abstract class MultiPlayerNetworkHandler : INetworkHandler
{
    public readonly ENetHost Host;

    public MultiPlayerNetworkHandler(ENetHost host)
    {
        Host = host;
    }

    public void Update()
    {
        var first = false;
        while (true)
        {
            var evt = Host.Service(TimeSpan.FromMilliseconds(first ? 1 : 0));
            first = false;
            if(evt.Type == ENetEventType.None) break;
            // Console.WriteLine($"Network event of type: {evt.Type}");
            ProcessEvent(evt);
        }
    }

    public abstract bool ShouldStop();
    public abstract void SendPacket<T>(PlayerEntity? player, T packet) where T : class, IPacket;


    protected abstract void Connect(ENetEvent evt);

    protected abstract void Disconnect(ENetEvent evt);

    protected abstract void Receive(ENetEvent evt);


    private void ProcessEvent(ENetEvent evt)
    {
        if (evt.Type == ENetEventType.Connect)
        {
            Connect(evt);
        } else if (evt.Type == ENetEventType.Disconnect)
        {
            Disconnect(evt);
        }
        else
        {
            Receive(evt);
            evt.Packet.Destroy();
        }
    }

    public void Dispose()
    {
        Host.Dispose();
    }
}