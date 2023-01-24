using BlockFactory.Side_;

namespace BlockFactory.Server.Dedicated;

[ExclusiveTo(Side.Server)]
public class BlockFactoryDedicatedServer : BlockFactoryServer
{
    private Thread _consoleInteractorThread = null!;
    public ConsoleInteractor ConsoleInteractor { get; private set; } = null!;

    public override void Init()
    {
        base.Init();
        ConsoleInteractor = new ConsoleInteractor(this);
        _consoleInteractorThread = new Thread(ConsoleInteractor.Run);
        _consoleInteractorThread.Start();
    }

    public override void Update()
    {
        base.Update();
        if (ConsoleInteractor.IsRunning)
            while (ConsoleInteractor.Commands.TryDequeue(out var result))
                ProcessCommand(ConsoleInteractor, result);
        else
            IsRunning = false;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        Environment.Exit(0);
    }
}