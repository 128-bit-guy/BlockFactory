using System.Net;
using BlockFactory.Base;
using BlockFactory.Network;
using ENet.Managed;

namespace BlockFactory.Server;

[ExclusiveTo(Side.Server)]
public static class BlockFactoryServer
{
    public static ServerNetworkHandler NetworkHandler;
    public static LogicProcessor LogicProcessor;
    private static int GetPort()
    {
        var x = Console.ReadLine()!;
        if (x.Length == 0) x = "" + Constants.DefaultPort;

        var port = int.Parse(x);
        return port;
    }

    private static void Init()
    {
        ManagedENet.Startup();
        BfContent.Init();
        var port = GetPort();
        NetworkHandler = new ServerNetworkHandler(port);
        LogicProcessor = new LogicProcessor(NetworkHandler, "world_server");
    }

    private static void Update()
    {
        LogicProcessor.Update();
        Thread.Sleep(1);
    }

    private static void Shutdown()
    {
        LogicProcessor.Dispose();
    }
    public static void Run()
    {
        Init();
        while (!LogicProcessor.ShouldStop())
        {
            Update();
        }
        Shutdown();
        // var listenEndPoint = new IPEndPoint(IPAddress.Loopback, port);
        // var host = new ENetHost(listenEndPoint, 16, 1, 0, 0);
        // SpinWait wait = default;
        // while (true)
        // {
        //     var evt = host.Service(TimeSpan.Zero);
        //     if (evt.Type == ENetEventType.Connect)
        //     {
        //         Console.WriteLine($"New connection: {evt.Peer.GetRemoteEndPoint()}");
        //         evt.Peer.Disconnect(123);
        //         
        //     }
        //     wait.SpinOnce();
        // }
        ManagedENet.Shutdown();
    }
}