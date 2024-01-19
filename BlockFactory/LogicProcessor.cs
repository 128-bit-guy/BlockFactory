using System.Diagnostics;
using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Network;
using BlockFactory.Network.Packet_;
using BlockFactory.World_;

namespace BlockFactory;

public class LogicProcessor : IDisposable
{
    public readonly LogicalSide LogicalSide;
    public readonly INetworkHandler NetworkHandler;
    private World _world = null!;
    private DateTime _lastTickTime;
    public DateTime LastUpdateTime;
    private readonly List<PlayerEntity> _players = new();
    private readonly List<Chunk>[] _chunkUpdateClasses = new List<Chunk>[27];
    private int _heavyUpdateClass;
    public readonly string SaveLocation;
    private readonly Stopwatch _stopwatch = new();

    public LogicProcessor(LogicalSide logicalSide, INetworkHandler networkHandler, string saveLocation)
    {
        NetworkHandler = networkHandler;
        LogicalSide = logicalSide;
        SaveLocation = saveLocation;
        for (var i = 0; i < 27; ++i)
        {
            _chunkUpdateClasses[i] = new List<Chunk>();
        }
    }

    public void Start()
    {
        _world = new World(this, SaveLocation);
        _lastTickTime = DateTime.UtcNow;
        NetworkHandler.Start();
    }

    private void UpdateChunk(Chunk c)
    {
        c.Update(c.GetUpdateClass() == _heavyUpdateClass);
    }

    private void Tick()
    {
        NetworkHandler.Update();
        _world.Update();
        foreach (var chunk in _world.GetLoadedChunks())
        {
            if (chunk.ReadyForTick)
            {
                _chunkUpdateClasses[chunk.GetUpdateClass()].Add(chunk);
            }
        }
        for (var i = 0; i < 27; ++i)
        {
            _chunkUpdateClasses[i].Shuffle(Random.Shared);
            Parallel.ForEach(_chunkUpdateClasses[i], UpdateChunk);
            _chunkUpdateClasses[i].Clear();
        }
        ++_heavyUpdateClass;
        if (_heavyUpdateClass == 27)
        {
            _heavyUpdateClass = 0;
        }
        foreach (var player in _players)
        {
            player.Update();
        }
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
        } else if (LogicalSide == LogicalSide.Server)
        {
            var packet = new ServerTickTimePacket(delta);
            foreach (var player in _players)
            {
                NetworkHandler.SendPacket(player, packet);
            }
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

    public World GetWorld()
    {
        return _world;
    }

    public bool ShouldStop()
    {
        return NetworkHandler.ShouldStop();
    }

    public void Dispose()
    {
        _world.Dispose();
        NetworkHandler.Dispose();
    }
}