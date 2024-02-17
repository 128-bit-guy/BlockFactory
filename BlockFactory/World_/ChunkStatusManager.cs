using System.Collections.Concurrent;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class ChunkStatusManager
{
    public delegate void ChunkEventHandler(Chunk c);

    public readonly World World;
    private readonly ConcurrentQueue<Chunk> _statusUpdateQueue = new();
    private readonly ConcurrentQueue<Chunk> _tickingUpdateQueue = new();

    public ChunkStatusManager(World world)
    {
        World = world;
    }

    public event ChunkEventHandler ChunkReadyForUse = c => { };
    public event ChunkEventHandler ChunkNotReadyForUse = c => { };
    public event ChunkEventHandler ChunkReadyForTick = c => { };
    public event ChunkEventHandler ChunkNotReadyForTick = c => { };

    public void OnChunkReadyForUse(Chunk c)
    {
        c.ReadyForUse = true;
        ChunkReadyForUse(c);
        ++c.ReadyForUseNeighbours;
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = c.Position + new Vector3D<int>(i, j, k);
            var oChunk = c.Neighbourhood.GetChunk(oPos, false);
            if (oChunk is not { ReadyForUse: true }) continue;
            ++oChunk.ReadyForUseNeighbours;
            if (oChunk.ReadyForUseNeighbours == 27)
            {
                oChunk.ReadyForTick = true;
                OnChunkReadyForTick(oChunk);
            }

            if (oChunk.ReadyForUse) ++c.ReadyForUseNeighbours;
        }

        if (c.ReadyForUseNeighbours == 27)
        {
            c.ReadyForTick = true;
            OnChunkReadyForTick(c);
        }
    }

    private void OnChunkReadyForTick(Chunk c)
    {
        ChunkReadyForTick(c);
        ScheduleTickingUpdate(c);
    }

    public void OnChunkNotReadyForUse(Chunk c)
    {
        if (c.ReadyForUseNeighbours == 27)
        {
            ChunkNotReadyForTick(c);
            c.ReadyForTick = false;
        }

        --c.ReadyForUseNeighbours;

        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = c.Position + new Vector3D<int>(i, j, k);
            var oChunk = c.Neighbourhood.GetChunk(oPos, false);
            if (oChunk is not { ReadyForUse: true }) continue;
            if (oChunk.ReadyForUseNeighbours == 27)
            {
                ChunkNotReadyForTick(oChunk);
                oChunk.ReadyForTick = false;
            }

            --oChunk.ReadyForUseNeighbours;

            if (oChunk.ReadyForUse) --c.ReadyForUseNeighbours;
        }

        ChunkNotReadyForUse(c);
        c.ReadyForUse = false;
    }

    private void UpdateStatus(Chunk chunk)
    {
        if (chunk.WatchingPlayers.Count == 0 && chunk.TickingDependencies == 0)
        {
            World.RemoveChunk(chunk.Position);
        }
        else
        {
            if (chunk is { IsLoaded: true, ReadyForUse: false })
            {
                chunk.LoadTask = null;
                OnChunkReadyForUse(chunk);
            }
        }
    }

    private void UpdateTicking(Chunk chunk)
    {
        if (chunk.IsTicking) return;
        if (!chunk.ShouldTick()) return;
        World.LogicProcessor.AddTickingChunk(chunk);
        chunk.IsTicking = true;
    }

    public void ScheduleStatusUpdate(Chunk c)
    {
        _statusUpdateQueue.Enqueue(c);
    }

    public void ScheduleTickingUpdate(Chunk c)
    {
        _tickingUpdateQueue.Enqueue(c);
    }

    public void Update()
    {
        {
            var cnt = _statusUpdateQueue.Count;
            for (var i = 0; i < cnt; ++i)
            {
                if (!_statusUpdateQueue.TryDequeue(out var c)) continue;
                if (!c.IsValid) continue;
                UpdateStatus(c);
            }
        }
        {
            var cnt = _tickingUpdateQueue.Count;
            for (var i = 0; i < cnt; ++i)
            {
                if (!_tickingUpdateQueue.TryDequeue(out var c)) continue;
                if (!c.IsValid) continue;
                UpdateTicking(c);
            }
        }
    }
}