using System.Collections.Concurrent;
using BlockFactory.Side_;

namespace BlockFactory.Server.Dedicated;

[ExclusiveTo(Side.Server)]
public class ConsoleInteractor
{
    public readonly ConcurrentQueue<string> Commands;
    public readonly BlockFactoryDedicatedServer Server;

    public ConsoleInteractor(BlockFactoryDedicatedServer server)
    {
        Server = server;
        Commands = new ConcurrentQueue<string>();
        IsRunning = true;
    }

    public bool IsRunning { get; private set; }

    private void ProcessCommand()
    {
        var command = Console.ReadLine()!;
        Commands.Enqueue(command);
    }

    public void Run()
    {
        try
        {
            while (true) ProcessCommand();
        }
        catch (Exception ex)
        {
            IsRunning = false;
        }
    }
}