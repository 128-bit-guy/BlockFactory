using BlockFactory.Side_;

namespace BlockFactory.Server;

public class BlockFactoryServer
{
    [ExclusiveTo(Side.Client)] public string AdminName = "LOLOSHKA!";

    private DateTime NextTickTime;
    public bool IsRunning { get; protected set; }

    public virtual void Init()
    {
        IsRunning = true;
    }

    public virtual void Update()
    {
    }

    public virtual void Shutdown()
    {
    }

    public void ProcessCommand(object sender, string command)
    {
        if (command.StartsWith("/")) command = command[1..];

        if (command == "stop")
            IsRunning = false;
        else if (command.StartsWith("say ")) Console.WriteLine(command[4..]);
    }

    public void Run()
    {
        Init();
        while (IsRunning)
            if (DateTime.UtcNow >= NextTickTime)
            {
                Update();
                NextTickTime += TimeSpan.FromSeconds(1 / 20f);
            }
            else
            {
                Thread.Sleep(1);
            }

        Shutdown();
    }
}