using System.Diagnostics;
using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Network;
using BlockFactory.Network.Packet_;
using BlockFactory.Registry_;
using BlockFactory.Serialization;
using BlockFactory.Server;
using BlockFactory.Utils;
using BlockFactory.World_;
using BlockFactory.World_.Search;
using Silk.NET.Maths;

namespace BlockFactory;

public class LogicProcessor : IDisposable
{
    private readonly List<Chunk>[] _chunkUpdateClasses = new List<Chunk>[27];
    private readonly List<PlayerEntity> _players = new();
    private readonly Stopwatch _stopwatch = new();
    public readonly LogicalSide LogicalSide;
    public readonly INetworkHandler NetworkHandler;
    public readonly PlayerData PlayerData;
    public readonly string SaveLocation;
    public readonly WorldData WorldData;
    private int _heavyUpdateClass;
    private DateTime _lastTickTime;
    private bool _stopRequested;
    private World _world = null!;
    public DateTime LastUpdateTime;
    private SpawnPointSearcher _spawnPointSearcher = null!;

    public LogicProcessor(LogicalSide logicalSide, INetworkHandler networkHandler, string saveLocation,
        WorldSettings? worldSettings = null)
    {
        NetworkHandler = networkHandler;
        LogicalSide = logicalSide;
        SaveLocation = saveLocation;
        LoadMapping();
        for (var i = 0; i < 27; ++i) _chunkUpdateClasses[i] = new List<Chunk>();

        if (logicalSide == LogicalSide.Client)
        {
            WorldData = new WorldData();
        }
        else if (worldSettings == null)
        {
            WorldData = new WorldData();
            TagIO.Deserialize(GetWorldDataLocation(), WorldData);
        }
        else
        {
            WorldData = new WorldData(worldSettings);
        }

        PlayerData = new PlayerData(this);
        if (logicalSide != LogicalSide.Client) TagIO.Deserialize(GetPlayerDataLocation(), PlayerData);
    }

    public void Dispose()
    {
        _world.Dispose();
        NetworkHandler.Dispose();
        if (LogicalSide != LogicalSide.Client)
        {
            TagIO.Serialize(GetWorldDataLocation(), WorldData);
            TagIO.Serialize(GetPlayerDataLocation(), PlayerData);
        }
    }

    public void Start()
    {
        _world = new World(this, SaveLocation);
        _lastTickTime = DateTime.UtcNow;
        NetworkHandler.Start();
        if (LogicalSide != LogicalSide.Client)
        {
            _spawnPointSearcher = new SpawnPointSearcher(_world);
        }
    }

    private string GetWorldDataLocation()
    {
        return Path.Combine(SaveLocation, "world.dat");
    }

    private string GetPlayerDataLocation()
    {
        return Path.Combine(SaveLocation, "players.dat");
    }

    private void UpdateChunk(Chunk c)
    {
        if (!c.ChunkStatusInfo.IsTicking) return;

        c.Update(c.GetUpdateClass() == _heavyUpdateClass);
    }

    public PlayerEntity GetOrCreatePlayer(string name)
    {
        if (PlayerData.Players.TryGetValue(name, out var p))
        {
            return p;
        }

        PlayerEntity player = LogicalSide == LogicalSide.Server ? new ServerPlayerEntity() : new ClientPlayerEntity();
        player.HeadRotation = new Vector2D<float>((float)Random.Shared.NextDouble() * 2 * MathF.PI,
            (float)Random.Shared.NextDouble() * MathF.PI - MathF.PI / 2);
        PlayerData.Players.Add(name, player);
        return player;
    }

    private void PreUpdateChunk(Chunk c)
    {
        if (!c.ChunkStatusInfo.ShouldTick())
        {
            c.ChunkStatusInfo.IsTicking = false;
            return;
        }

        c.PreUpdate();
    }

    public void AddTickingChunk(Chunk c)
    {
        _chunkUpdateClasses[c.GetUpdateClass()].Add(c);
    }

    private void Tick()
    {
        NetworkHandler.Update();
        _world.Update();
        for (var i = 0; i < 27; ++i)
        {
            _chunkUpdateClasses[i].Shuffle(Random.Shared);
            Parallel.ForEach(_chunkUpdateClasses[i], PreUpdateChunk);
        }

        for (var i = 0; i < 27; ++i)
        {
            Parallel.ForEach(_chunkUpdateClasses[i], UpdateChunk);
            _chunkUpdateClasses[i].RemoveAll(c => !c.ChunkStatusInfo.IsTicking);
        }

        ++_heavyUpdateClass;
        if (_heavyUpdateClass == 27) _heavyUpdateClass = 0;

        if (LogicalSide != LogicalSide.Client && WorldData.SpawnPoint == null)
        {
            _spawnPointSearcher.Update();
            WorldData.SpawnPoint = _spawnPointSearcher.FoundPos;
        }

        foreach (var player in _players) player.Update();
    }

    [ExclusiveTo(Side.Client)]
    public double GetPartialTicks()
    {
        return (LastUpdateTime - _lastTickTime).TotalMilliseconds / Constants.TickFrequencyMs;
    }

    public void AddPlayer(PlayerEntity player)
    {
        _players.Add(player);
    }

    public void RemovePlayer(PlayerEntity player)
    {
        _players.Remove(player);
    }

    public IEnumerable<PlayerEntity> GetPlayers()
    {
        return _players;
    }

    private void HandleTickTime(float delta)
    {
        if (LogicalSide == LogicalSide.SinglePlayer)
        {
            BfDebug.HandleTickTime(delta);
        }
        else if (LogicalSide == LogicalSide.Server)
        {
            var packet = new ServerTickTimePacket(delta);
            foreach (var player in _players) NetworkHandler.SendPacket(player, packet);
        }
    }

    public void Update()
    {
        var now = DateTime.UtcNow;
        LastUpdateTime = now;
        if (_lastTickTime + TimeSpan.FromMilliseconds(Constants.TickFrequencyMs) < now)
        {
            _stopwatch.Restart();
            Tick();
            _stopwatch.Stop();
            HandleTickTime((float)_stopwatch.Elapsed.TotalMilliseconds);
            _lastTickTime += TimeSpan.FromMilliseconds(Constants.TickFrequencyMs);
        }
    }

    public void FastTick(int ticks)
    {
        _lastTickTime -= TimeSpan.FromMilliseconds(Constants.TickFrequencyMs * ticks);
    }

    public World GetWorld()
    {
        return _world;
    }

    public bool ShouldStop()
    {
        return _stopRequested || NetworkHandler.ShouldStop();
    }

    public void RequestStop()
    {
        _stopRequested = true;
    }

    private string GetMappingSaveLocation()
    {
        return Path.Combine(SaveLocation, "registry_mapping.dat");
    }

    private void LoadMapping()
    {
        var mappingSaveLocation = GetMappingSaveLocation();
        var mapping = new RegistryMapping();
        if (File.Exists(mappingSaveLocation)) TagIO.Deserialize(mappingSaveLocation, mapping);

        SynchronizedRegistries.LoadMapping(mapping);
    }

    public void SaveMapping()
    {
        var mappingSaveLocation = GetMappingSaveLocation();
        TagIO.Serialize(mappingSaveLocation, SynchronizedRegistries.WriteMapping());
    }
}