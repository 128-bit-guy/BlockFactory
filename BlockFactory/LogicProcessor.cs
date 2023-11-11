using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.World_;

namespace BlockFactory;

public class LogicProcessor : IDisposable
{
    private World _world = null!;
    private DateTime _lastTickTime;
    private readonly List<PlayerEntity> _players = new();

    public LogicProcessor()
    {
        
    }

    public void Start()
    {
        _world = new World("world");
        _lastTickTime = DateTime.UtcNow;
    }

    private void Tick()
    {
        _world.Update();
        foreach (var player in _players)
        {
            player.Update();
        }
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

    public void Dispose()
    {
        _world.Dispose();
    }
}