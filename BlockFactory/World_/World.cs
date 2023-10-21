using BlockFactory.Base;
using BlockFactory.Math_;
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

    public short GetBlock(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBlock(pos);
    }
    
    public byte GetBiome(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBiome(pos);
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBlock(pos, block, update);
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBiome(pos, biome);
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

                if (chunk.ReadyForTick) chunk.Update();
            }

        foreach (var chunk in _chunksToRemove) RemoveChunk(chunk.Position);
        _chunksToRemove.Clear();
        SaveManager.Update();
    }

    public void Dispose()
    {
        _chunksToRemove.AddRange(GetLoadedChunks());
        
        foreach (var chunk in _chunksToRemove) RemoveChunk(chunk.Position);
        
        _chunksToRemove.Clear();
        
        SaveManager.Dispose();
    }
}