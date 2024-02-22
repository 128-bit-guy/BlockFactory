using System.Collections.Concurrent;
using System.Diagnostics;
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
    private static ConcurrentQueue<string> ConsoleCommandQueue = new();
    private static Thread ConsoleCommandReaderThread;
    public static RegistryMapping Mapping;

    private static int GetPort()
    {
        var x = Console.ReadLine()!;
        if (x.Length == 0) x = "" + Constants.DefaultPort;

        var port = int.Parse(x);
        return port;
    }
    
    private static void ReadConsoleCommands()
    {
        while (true)
        {
            var s = Console.ReadLine();
            if (s != null)
            {
                ConsoleCommandQueue.Enqueue(s);
            }
        }
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
        Mapping = SynchronizedRegistries.WriteMapping();
        ConsoleCommandReaderThread = new Thread(ReadConsoleCommands);
        ConsoleCommandReaderThread.IsBackground = true;
        ConsoleCommandReaderThread.Start();
    }

    private static void ExecuteConsoleCommands()
    {
        var cnt = ConsoleCommandQueue.Count;
        for (var i = 0; i < cnt; ++i)
        {
            if (ConsoleCommandQueue.TryDequeue(out var res))
            {
                if (res == "/stop")
                {
                    LogicProcessor.RequestStop();
                }
                else
                {
                    Console.WriteLine($"Unknown command: {res}");
                }
            }
            else
            {
                break;
            }
        }
    }

    private static void Update()
    {
        ExecuteConsoleCommands();
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