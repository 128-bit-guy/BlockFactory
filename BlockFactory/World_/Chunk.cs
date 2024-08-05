﻿using System.Collections;
using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Content.Entity_;
using BlockFactory.Network.Packet_;
using BlockFactory.Utils;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class Chunk : IBlockWorld
{
    public delegate void BlockEventHandler(Vector3D<int> pos);

    #region Updates
    private readonly List<Vector3D<int>> _blockUpdateBuffer = new();
    private readonly BitArray _bufferedUpdateScheduled =
        new(Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize);
    private readonly List<Vector3D<int>> _scheduledBlockUpdates = new();
    public readonly List<Vector3D<int>> ScheduledLightUpdates = new();
    #endregion
    
    #region Status

    #endregion

    public readonly ChunkNeighbourhood Neighbourhood;
    
    public readonly Vector3D<int> Position;
    public readonly ChunkRegion? Region;
    public readonly World World;
    public ChunkData? Data;
    public readonly ChunkStatusInfo ChunkStatusInfo;


    public Chunk(World world, Vector3D<int> position, ChunkRegion? region)
    {
        Position = position;
        Region = region;
        World = world;
        Neighbourhood = new ChunkNeighbourhood(this);
        ChunkStatusInfo = new ChunkStatusInfo(this);
    }

    public short GetBlock(Vector3D<int> pos)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        return Data!.GetBlock(pos);
    }

    public byte GetBiome(Vector3D<int> pos)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        return Data!.GetBiome(pos);
    }

    public byte GetLight(Vector3D<int> pos, LightChannel channel)
    {
        return Data!.GetLight(pos, channel);
    }

    public bool IsBlockLoaded(Vector3D<int> pos)
    {
        return pos.ShiftRight(Constants.ChunkSizeLog2) == Position;
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        if (Data!.GetBlock(pos) == block) return;
        Data!.SetBlock(pos, block, update);
        _bufferedUpdateScheduled[GetArrIndex(pos)] = false;
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, new BlockChangePacket(pos, block));
            }

        if (!update) return;
        ScheduleLightUpdate(pos);
        UpdateBlock(pos);
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            var oPos = pos + new Vector3D<int>(i, j, k);
            var oc = Neighbourhood.GetChunk(oPos.ShiftRight(Constants.ChunkSizeLog2), false);
            if (World.LogicProcessor.LogicalSide == LogicalSide.Client && oc == null) continue;
            Neighbourhood.UpdateBlock(oPos);
        }
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        Data!.SetBiome(pos, biome);
    }

    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light)
    {
        if (Data!.GetLight(pos, channel) == light) return;
        Data!.SetLight(pos, channel, light);
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, new LightChangePacket(pos, channel, light));
            }

        Neighbourhood.UpdateLight(pos);
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
        if (World.LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            ChunkStatusInfo.LoadTask?.Wait();
            if (!Data!.IsBlockUpdateScheduled(pos))
            {
                Data!.SetBlockUpdateScheduled(pos, true);
                _scheduledBlockUpdates.Add(pos);
            }
        }

        BlockUpdate(pos);
    }

    public void ScheduleLightUpdate(Vector3D<int> pos)
    {
        if (!Data!.IsLightUpdateScheduled(pos))
        {
            Data!.SetLightUpdateScheduled(pos, true);
            ScheduledLightUpdates.Add(pos);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & Constants.ChunkMask) | (((pos.Y & Constants.ChunkMask) | ((pos.Z & Constants.ChunkMask)
            << Constants.ChunkSizeLog2)) << Constants.ChunkSizeLog2);
    }

    private void MoveBlockUpdatesToBuffer()
    {
        _blockUpdateBuffer.AddRange(_scheduledBlockUpdates);
        foreach (var pos in _scheduledBlockUpdates)
        {
            _bufferedUpdateScheduled[GetArrIndex(pos)] = true;
            Data!.SetBlockUpdateScheduled(pos, false);
        }

        _scheduledBlockUpdates.Clear();
    }

    private void UpdateBlocksInBuffer()
    {
        foreach (var pos in _blockUpdateBuffer)
        {
            if (!_bufferedUpdateScheduled[GetArrIndex(pos)]) continue;
            this.GetBlockObj(pos).UpdateBlock(new BlockPointer(Neighbourhood, pos));
            _bufferedUpdateScheduled[GetArrIndex(pos)] = false;
        }

        _blockUpdateBuffer.Clear();
    }

    public void UpdateLight(Vector3D<int> pos)
    {
        LightUpdate(pos);
    }

    public event BlockEventHandler BlockUpdate = p => { };
    public event BlockEventHandler LightUpdate = p => { };

    private void FullyDecoratedUpdate()
    {
        var x = World.Random.Next(Constants.ChunkSize);
        var y = World.Random.Next(Constants.ChunkSize);
        var z = World.Random.Next(Constants.ChunkSize);
        var absPos = new Vector3D<int>(x, y, z) + Position.ShiftLeft(Constants.ChunkSizeLog2);
        this.GetBlockObj(absPos).RandomUpdateBlock(new BlockPointer(Neighbourhood, absPos));
        UpdateBlocksInBuffer();
    }

    public void PreUpdate()
    {
        if (Data!.FullyDecorated) MoveBlockUpdatesToBuffer();
    }

    public void Update(bool heavy)
    {
        if (World.LogicProcessor.LogicalSide == LogicalSide.Client) return;
        var wasInitialized = Data!.Decorated && Data!.HasSkyLight;
        if (heavy && !Data!.Decorated)
        {
            Data!.Decorated = true;
            World.Generator.DecorateChunk(this);
        }

        if (!Data!.Decorated) return;

        if (Data!.FullyDecorated) FullyDecoratedUpdate();

        if (heavy || Data!.HasSkyLight) LightPropagator.ProcessLightUpdates(this);

        if (wasInitialized) return;
        var isInitialized = Data!.Decorated && Data!.HasSkyLight;
        if (!isInitialized) return;

        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
            ++Neighbourhood.GetChunk(Position + new Vector3D<int>(i, j, k))!.Data!.DecoratedNeighbours;
    }

    private void CopyUpdatesFromData()
    {
        ScheduledLightUpdates.Clear();
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var absPos = new Vector3D<int>(i, j, k) + Position.ShiftLeft(Constants.ChunkSizeLog2);
            if (Data!.IsLightUpdateScheduled(absPos)) ScheduledLightUpdates.Add(absPos);

            if (Data!.IsBlockUpdateScheduled(absPos)) _scheduledBlockUpdates.Add(absPos);
        }
    }

    public void GenerateOrLoad()
    {
        var data = Region!.GetChunk(Position);
        if (data == null)
        {
            World.Generator.GenerateChunk(this);
            Region.SetChunk(Position, Data!);
        }
        else
        {
            Data = data;
        }

        CopyUpdatesFromData();

        ChunkStatusInfo.SetLoadingCompleted();

        World.ChunkStatusManager.ScheduleStatusUpdate(this);
    }

    public int GetUpdateClass()
    {
        var x = Position.X % 3;
        if (x < 0) x += 3;
        var y = Position.Y % 3;
        if (y < 0) y += 3;
        var z = Position.Z % 3;
        if (z < 0) z += 3;
        return x + y * 3 + z * 9;
    }
}