using System.Collections.Concurrent;
using BlockFactory.Base;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;
using BlockFactory.World_;
using ZstdSharp;

namespace BlockFactory.Game;

public class GameInstance : IDisposable
{
    private readonly ConcurrentQueue<Action> Actions = new();
    public readonly GameKind Kind;
    public readonly string PlayerDataLocation;
    public readonly string SaveLocation;
    private DateTime _nextTickTime;
    public Thread MainThread;
    public INetworkHandler NetworkHandler = null!;
    public PlayerManager PlayerManager;
    public Random Random;
    public ISideHandler SideHandler = null!;
    public World World;

    public GameInstance(GameKind kind, Thread mainThread, string saveLocation)
    {
        Kind = kind;
        MainThread = mainThread;
        Random = new Random();
        SaveLocation = saveLocation;
        World = new World(this, saveLocation);
        World.InitGenerator();
        PlayerManager = new PlayerManager(this);
        PlayerDataLocation = Path.Combine(SaveLocation, "players.dat");
        if (kind.DoesProcessLogic()) LoadGlobalData();
    }

    public GameInstance(GameKind kind, Thread mainThread, string saveLocation, int seed)
    {
        Kind = kind;
        MainThread = mainThread;
        Random = new Random();
        SaveLocation = saveLocation;
        World = new World(this, saveLocation, seed);
        World.InitGenerator();
        PlayerManager = new PlayerManager(this);
        PlayerDataLocation = Path.Combine(SaveLocation, "players.dat");
        if (kind.DoesProcessLogic()) LoadGlobalData();
    }

    public static bool Exists(string saveLocation)
    {
        return Directory.Exists(saveLocation);
    }

    public void Dispose()
    {
        if (Kind.DoesProcessLogic()) SaveGlobalData();

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

    public void LoadGlobalData()
    {
        TagIO.Deserialize(PlayerDataLocation, PlayerManager);
    }

    public void SaveGlobalData()
    {
        TagIO.Serialize(PlayerDataLocation, PlayerManager);
        World.SaveData();
    }
}