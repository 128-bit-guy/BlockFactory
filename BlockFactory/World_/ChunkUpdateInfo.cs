using System.Collections;
using BlockFactory.Base;
using BlockFactory.Utils;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class ChunkUpdateInfo
{
    private readonly Chunk _chunk;
    private readonly List<Vector3D<int>> _blockUpdateBuffer = new();

    private readonly BitArray _bufferedUpdateScheduled =
        new(Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize);

    private readonly List<Vector3D<int>> _scheduledBlockUpdates = new();
    public readonly List<Vector3D<int>> ScheduledLightUpdates = new();

    public ChunkUpdateInfo(Chunk chunk)
    {
        _chunk = chunk;
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
        if (_chunk.World.LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            _chunk.ChunkStatusInfo.LoadTask?.Wait();
            if (!_chunk.Data!.IsBlockUpdateScheduled(pos))
            {
                _chunk.Data!.SetBlockUpdateScheduled(pos, true);
                _scheduledBlockUpdates.Add(pos);
            }
        }

        BlockUpdate(pos);
    }

    public void OnBlockChanged(Vector3D<int> pos)
    {
        _bufferedUpdateScheduled[Chunk.GetArrIndex(pos)] = false;
    }

    public void ScheduleLightUpdate(Vector3D<int> pos)
    {
        if (!_chunk.Data!.IsLightUpdateScheduled(pos))
        {
            _chunk.Data!.SetLightUpdateScheduled(pos, true);
            ScheduledLightUpdates.Add(pos);
        }
    }

    public delegate void BlockEventHandler(Vector3D<int> pos);

    public void MoveBlockUpdatesToBuffer()
    {
        _blockUpdateBuffer.AddRange(_scheduledBlockUpdates);
        foreach (var pos in _scheduledBlockUpdates)
        {
            _bufferedUpdateScheduled[Chunk.GetArrIndex(pos)] = true;
            _chunk.Data!.SetBlockUpdateScheduled(pos, false);
        }

        _scheduledBlockUpdates.Clear();
    }

    public void UpdateBlocksInBuffer()
    {
        foreach (var pos in _blockUpdateBuffer)
        {
            if (!_bufferedUpdateScheduled[Chunk.GetArrIndex(pos)]) continue;
            _chunk.GetBlockObj(pos).UpdateBlock(new BlockPointer(_chunk.Neighbourhood, pos));
            _bufferedUpdateScheduled[Chunk.GetArrIndex(pos)] = false;
        }

        _blockUpdateBuffer.Clear();
    }

    public void UpdateLight(Vector3D<int> pos)
    {
        LightUpdate(pos);
    }

    public event BlockEventHandler BlockUpdate = p => { };
    public event BlockEventHandler LightUpdate = p => { };

    public void CopyUpdatesFromData()
    {
        ScheduledLightUpdates.Clear();
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var absPos = new Vector3D<int>(i, j, k) + _chunk.Position.ShiftLeft(Constants.ChunkSizeLog2);
            if (_chunk.Data!.IsLightUpdateScheduled(absPos)) ScheduledLightUpdates.Add(absPos);

            if (_chunk.Data!.IsBlockUpdateScheduled(absPos)) _scheduledBlockUpdates.Add(absPos);
        }
    }
}