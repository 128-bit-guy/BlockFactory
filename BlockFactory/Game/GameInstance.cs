using System.Collections.Concurrent;
using BlockFactory.Base;
using BlockFactory.World_;

namespace BlockFactory.Game;

public class GameInstance : IDisposable
{
    private readonly ConcurrentQueue<Action> Actions = new();
    public readonly GameKind Kind;
    public readonly string SaveLocation;
    private DateTime _nextTickTime;
    public Thread MainThread;
    public INetworkHandler NetworkHandler = null!;
    public Random Random;
    public ISideHandler SideHandler = null!;
    public World World;

    public GameInstance(GameKind kind, Thread mainThread, int seed, string saveLocation)
    {
        Kind = kind;
        MainThread = mainThread;
        Random = new Random();
        SaveLocation = saveLocation;
        World = new World(this, seed, saveLocation);
    }

    public void Dispose()
    {
        World.Dispose();
    }

    public void Init()
    {
        _nextTickTime = DateTime.UtcNow;
    }


    private void Tick()
    {
        World.Tick();
        if (Kind.IsNetworked())
        {
            if (Kind.DoesProcessLogic())
                foreach (var connection in NetworkHandler.GetAllConnections())
                    connection.Flush();
            else
                NetworkHandler.GetServerConnection().Flush();
        }
    }

    public bool Update()
    {
        ProcessScheduled();
        if (DateTime.UtcNow >= _nextTickTime)
        {
            _nextTickTime += Constants.TickPeriod;
            Tick();
            return true;
        }

        return false;
    }

    public void Wait(Task task)
    {
        task.Wait();
    }

    public void ProcessScheduled()
    {
        var cnt = Actions.Count;
        for (var i = 0; i < cnt; ++i)
            if (Actions.TryDequeue(out var action))
                action();
    }

    public void EnqueueWork(Action action)
    {
        Actions.Enqueue(action);
    }
}