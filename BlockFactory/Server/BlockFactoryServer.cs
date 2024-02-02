using System.Net;
using BlockFactory.Base;
using BlockFactory.Network;
using BlockFactory.Registry_;
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
        LogicProcessor = new LogicProcessor(LogicalSide.Server, NetworkHandler, "world_server");
        LogicProcessor.LoadMapping();
        LogicProcessor.Start();
    }

    private static void Update()
    {
        LogicProcessor.Update();
        Thread.Sleep(1);
    }

    private static void Shutdown()
    {
        LogicProcessor.SaveMapping();
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
        ManagedENet.Shutdown();
    }
}