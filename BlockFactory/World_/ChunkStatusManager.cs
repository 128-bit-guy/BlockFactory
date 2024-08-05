using System.Collections.Concurrent;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class ChunkStatusManager
{
    public delegate void ChunkEventHandler(Chunk c);

    private readonly ConcurrentQueue<Chunk> _statusUpdateQueue = new();
    private readonly ConcurrentQueue<Chunk> _tickingUpdateQueue = new();

    public readonly World World;

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
        c.ChunkStatusInfo.ReadyForUse = true;
        ChunkReadyForUse(c);
        ++c.ChunkStatusInfo.ReadyForUseNeighbours;
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = c.Position + new Vector3D<int>(i, j, k);
            var oChunk = c.Neighbourhood.GetChunk(oPos, false);
            if (oChunk is not { ChunkStatusInfo.ReadyForUse: true }) continue;
            ++oChunk.ChunkStatusInfo.ReadyForUseNeighbours;
            if (oChunk.ChunkStatusInfo.ReadyForUseNeighbours == 27)
            {
                oChunk.ChunkStatusInfo.ReadyForTick = true;
                OnChunkReadyForTick(oChunk);
            }

            if (oChunk.ChunkStatusInfo.ReadyForUse) ++c.ChunkStatusInfo.ReadyForUseNeighbours;
        }

        if (c.ChunkStatusInfo.ReadyForUseNeighbours == 27)
        {
            c.ChunkStatusInfo.ReadyForTick = true;
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
        if (c.ChunkStatusInfo.ReadyForUseNeighbours == 27)
        {
            ChunkNotReadyForTick(c);
            c.ChunkStatusInfo.ReadyForTick = false;
        }

        --c.ChunkStatusInfo.ReadyForUseNeighbours;

        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = c.Position + new Vector3D<int>(i, j, k);
            var oChunk = c.Neighbourhood.GetChunk(oPos, false);
            if (oChunk is not { ChunkStatusInfo.ReadyForUse: true }) continue;
            if (oChunk.ChunkStatusInfo.ReadyForUseNeighbours == 27)
            {
                ChunkNotReadyForTick(oChunk);
                oChunk.ChunkStatusInfo.ReadyForTick = false;
            }

            --oChunk.ChunkStatusInfo.ReadyForUseNeighbours;

            if (oChunk.ChunkStatusInfo.ReadyForUse) --c.ChunkStatusInfo.ReadyForUseNeighbours;
        }

        ChunkNotReadyForUse(c);
        c.ChunkStatusInfo.ReadyForUse = false;
    }

    private void UpdateStatus(Chunk chunk)
    {
        if (chunk.ChunkStatusInfo.WatchingPlayers.Count == 0 && chunk.ChunkStatusInfo.TickingDependencies == 0)
        {
            World.RemoveChunk(chunk.Position);
        }
        else
        {
            if (chunk is { ChunkStatusInfo.IsLoaded: true, ChunkStatusInfo.ReadyForUse: false })
            {
                chunk.ChunkStatusInfo.LoadTask = null;
                OnChunkReadyForUse(chunk);
            }
        }
    }

    private void UpdateTicking(Chunk chunk)
    {
        if (chunk.ChunkStatusInfo.IsTicking) return;
        if (!chunk.ChunkStatusInfo.ShouldTick()) return;
        World.LogicProcessor.AddTickingChunk(chunk);
        chunk.ChunkStatusInfo.IsTicking = true;
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
                if (!c.ChunkStatusInfo.IsValid) continue;
                UpdateStatus(c);
            }
        }
        {
            var cnt = _tickingUpdateQueue.Count;
            for (var i = 0; i < cnt; ++i)
            {
                if (!_tickingUpdateQueue.TryDequeue(out var c)) continue;
                if (!c.ChunkStatusInfo.IsValid) continue;
                UpdateTicking(c);
            }
        }
    }
}