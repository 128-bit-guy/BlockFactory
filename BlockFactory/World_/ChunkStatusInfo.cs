using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;

namespace BlockFactory.World_;

public class ChunkStatusInfo
{
    private readonly Chunk _chunk;
    public readonly HashSet<PlayerEntity> WatchingPlayers = new();
    private bool _loadingCompleted;
    public bool IsTicking = false;
    public bool IsValid = false;
    public Task? LoadTask;
    public bool ReadyForTick = false;
    public bool ReadyForUse = false;
    public int ReadyForUseNeighbours = 0;
    public int TickingDependencies;

    public ChunkStatusInfo(Chunk chunk)
    {
        _chunk = chunk;
    }

    public bool IsLoaded => (_chunk.Data != null && LoadTask == null) || LoadTask!.IsCompleted || _loadingCompleted;

    public void AddWatchingPlayer(PlayerEntity player)
    {
        WatchingPlayers.Add(player);
    }

    public void RemoveWatchingPlayer(PlayerEntity player)
    {
        WatchingPlayers.Remove(player);
        _chunk.World.ChunkStatusManager.ScheduleStatusUpdate(_chunk);
    }

    public void StartLoadTask()
    {
        if (_chunk.Region!.LoadTask == null)
            LoadTask = Task.Run(_chunk.GenerateOrLoad);
        else
            LoadTask = Task.Factory.ContinueWhenAll(new[] { _chunk.Region.LoadTask }, _ => _chunk.GenerateOrLoad());
    }

    public void SetLoadingCompleted()
    {
        _loadingCompleted = true;
    }

    public void AddTickingDependency()
    {
        ++TickingDependencies;
        _chunk.World.ChunkStatusManager.ScheduleTickingUpdate(_chunk);
    }

    public void RemoveTickingDependency()
    {
        --TickingDependencies;
    }

    public bool ShouldTick()
    {
        if (!IsValid || !ReadyForTick) return false;
        return !_chunk.Data!.Decorated || !_chunk.Data!.HasSkyLight || TickingDependencies > 0;
    }
}