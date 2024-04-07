using System.Collections.Concurrent;
using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Network;
using BlockFactory.Registry_;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Server;

[ExclusiveTo(Side.Server)]
public static class BlockFactoryServer
{
    public static ServerNetworkHandler NetworkHandler;
    public static LogicProcessor LogicProcessor;
    private static readonly ConcurrentQueue<string> ConsoleCommandQueue = new();
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
            if (s != null) ConsoleCommandQueue.Enqueue(s);
        }
    }

    private static void Init()
    {
        ManagedENet.Startup();
        BfContent.Init();
        var port = GetPort();
        NetworkHandler = new ServerNetworkHandler(port);
        LogicProcessor = new LogicProcessor(LogicalSide.Server, NetworkHandler, "world_server");
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
            if (ConsoleCommandQueue.TryDequeue(out var res))
            {
                ProcessCommand(null, res);
            }
            else
            {
                break;
            }
    }

    public static void ProcessCommand(PlayerEntity? sender, string command)
    {
        var s = command.Split(' ');
        if (s.Length == 0)
        {
            return;
        }
        if (s[0] == "/stop")
            LogicProcessor.RequestStop();
        else if (s[0] == "/tp")
        {
            try
            {
                Vector3D<double> newPos = default;
                for (int i = 0; i < 3; ++i)
                {
                    var n = s[i + 1];
                    var tilde = n[0] == '~';
                    if (tilde) n = n[1..];
                    var c = n == ""? 0 : double.Parse(n);
                    if (tilde)
                    {
                        c += sender!.Pos[i];
                    }
                    newPos.SetValue(i, c);
                }

                sender!.Pos = newPos;
                sender.Velocity = Vector3D<double>.Zero;
            }
            catch (Exception)
            {
                //Ignore incorrect command
            }
        }
        else if (s[0] == "/fly")
        {
            sender!.HasGravity = !sender.HasGravity;
            sender.SendUpdateToClient();
        } else if (s[0] == "/fast_tick")
        {
            try
            {
                var ticks = int.Parse(s[1]);
                LogicProcessor.FastTick(ticks);
            }
            catch (Exception)
            {
                //Ignore incorrect command
            }
        }
        else if(sender == null)
            Console.WriteLine($"Unknown command: {command}");
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
        while (!LogicProcessor.ShouldStop()) Update();

        Shutdown();
        ManagedENet.Shutdown();
    }
}