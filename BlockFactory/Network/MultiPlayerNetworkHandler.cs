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
            Console.WriteLine($"Network event of type: {evt.Type}");
            ProcessEvent(evt);
        }
    }

    public abstract bool ShouldStop();

    protected abstract void ProcessEvent(ENetEvent evt);
}