using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Network;
using BlockFactory.World_;

namespace BlockFactory;

public class LogicProcessor : IDisposable
{
    public readonly LogicalSide LogicalSide;
    public readonly INetworkHandler NetworkHandler;
    private World _world = null!;
    private DateTime _lastTickTime;
    private DateTime _lastUpdateTime;
    private readonly List<PlayerEntity> _players = new();
    private readonly List<Chunk>[] _chunkUpdateClasses = new List<Chunk>[27];
    private int _heavyUpdateClass;

    public LogicProcessor(LogicalSide logicalSide, INetworkHandler networkHandler, string saveLocation)
    {
        NetworkHandler = networkHandler;
        LogicalSide = logicalSide;
        for (var i = 0; i < 27; ++i)
        {
            _chunkUpdateClasses[i] = new List<Chunk>();
        }
        _world = new World(this, saveLocation);
        _lastTickTime = DateTime.UtcNow;
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
        return (_lastUpdateTime - _lastTickTime).TotalMilliseconds / Constants.TickFrequencyMs;
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

    public void Update()
    {
        var now = DateTime.UtcNow;
        _lastUpdateTime = now;
        if (_lastTickTime + TimeSpan.FromMilliseconds(Constants.TickFrequencyMs) < now)
        {
            Tick();
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