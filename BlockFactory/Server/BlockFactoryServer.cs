using BlockFactory.Side_;

namespace BlockFactory.Server;

public class BlockFactoryServer
{
    public bool IsRunning { get; protected set; }
    private DateTime NextTickTime;
    [ExclusiveTo(Side.Server)]
    private string ServerName { get; set; }

    public virtual void Init()
    {
        IsRunning = true;
        NextTickTime = DateTime.UtcNow;
    }

    public virtual void Update()
    {
        // Console.WriteLine("Tick!");
    }

    public virtual void Shutdown()
    {
    }

    public void ProcessCommand(object sender, string command)
    {
        if (command.StartsWith("/"))
        {
            command = command[1..];
        }

        if (command == "stop")
        {
            IsRunning = false;
        } else if (command.StartsWith("say "))
        {
            Console.WriteLine(command[4..]);
        }
    }

    public void Run()
    {
        Init();
        while (IsRunning)
        {
            if (DateTime.UtcNow >= NextTickTime)
            {
                Update();
                NextTickTime += TimeSpan.FromSeconds(1 / 20f);
            }
            else
            {
                Thread.Sleep(1);
            }
        }

        Shutdown();
    }
}