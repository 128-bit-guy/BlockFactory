﻿using BlockFactory.Base;
using BlockFactory.Math_;
using BlockFactory.World_.Gen;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class World : IChunkStorage, IBlockWorld, IDisposable
{
    private readonly List<Chunk> _chunksToRemove = new();
    public readonly ChunkStatusManager ChunkStatusManager;
    public readonly WorldChunkStorage ChunkStorage = new();
    public readonly WorldGenerator Generator = new();
    public readonly WorldSaveManager SaveManager;
    public readonly Random Random = new();
    private int _heavyUpdateIndex = 0;
    private readonly List<Chunk> _chunksToDoHeavyUpdate = new();

    public World(string saveLocation)
    {
        SaveManager = new WorldSaveManager(saveLocation);
        SaveManager.CreateDirectory();
        ChunkStatusManager = new ChunkStatusManager(this);
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.UpdateBlock(pos);
    }

    public void ScheduleLightUpdate(Vector3D<int> pos)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.ScheduleLightUpdate(pos);
    }

    public short GetBlock(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBlock(pos);
    }
    
    public byte GetBiome(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBiome(pos);
    }

    public byte GetLight(Vector3D<int> pos, LightChannel channel)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetLight(pos, channel);
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBlock(pos, block, update);
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBiome(pos, biome);
    }

    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetLight(pos, channel, light);
    }

    public Chunk? GetChunk(Vector3D<int> pos, bool load = true)
    {
        var res = ChunkStorage.GetChunk(pos, false);
        if (res != null || !load) return res;
        return BeginLoadingChunk(pos);
    }

    private Chunk BeginLoadingChunk(Vector3D<int> pos)
    {
        var region = SaveManager.GetRegion(pos.ShiftRight(ChunkRegion.SizeLog2));
        var nc = new Chunk(this, pos, region);
        ++region.DependencyCount;
        nc.StartLoadTask();
        AddChunk(nc);
        return nc;
    }

    public void AddChunk(Chunk chunk)
    {
        ChunkStorage.AddChunk(chunk);
    }

    public void RemoveChunk(Vector3D<int> pos)
    {
        var c = ChunkStorage.GetChunk(pos)!;
        c.LoadTask?.Wait();
        if (c.ReadyForUse) ChunkStatusManager.OnChunkNotReadyForUse(c);
        
        ChunkStorage.RemoveChunk(pos);

        --c.Region.DependencyCount;
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return ChunkStorage.GetLoadedChunks();
    }

    public void Update()
    {
        foreach (var chunk in GetLoadedChunks())
            if (chunk.WatchingPlayers.Count == 0)
            {
                _chunksToRemove.Add(chunk);
            }
            else
            {
                if (chunk is { IsLoaded: true, ReadyForUse: false })
                {
                    chunk.LoadTask = null;
                    ChunkStatusManager.OnChunkReadyForUse(chunk);
                }

                if (chunk.ReadyForTick)
                {
                    chunk.Update();
                    if(chunk.GetHeavyUpdateIndex() == _heavyUpdateIndex) _chunksToDoHeavyUpdate.Add(chunk);
                }
            }
        
        _chunksToDoHeavyUpdate.Shuffle(Random);

        Parallel.ForEach(_chunksToDoHeavyUpdate, LightPropagator.ProcessLightUpdates);
        
        _chunksToDoHeavyUpdate.Clear();

        foreach (var chunk in _chunksToRemove) RemoveChunk(chunk.Position);
        _chunksToRemove.Clear();
        SaveManager.Update();
        
        ++_heavyUpdateIndex;
        if (_heavyUpdateIndex == 27)
        {
            _heavyUpdateIndex = 0;
        }
    }

    public void Dispose()
    {
        _chunksToRemove.AddRange(GetLoadedChunks());
        
        foreach (var chunk in _chunksToRemove) RemoveChunk(chunk.Position);
        
        _chunksToRemove.Clear();
        
        SaveManager.Dispose();
    }
}