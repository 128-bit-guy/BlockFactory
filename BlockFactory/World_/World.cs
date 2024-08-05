using BlockFactory.Base;
using BlockFactory.Utils;
using BlockFactory.World_.Gen;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using BlockFactory.World_.Search;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class World : IChunkStorage, IBlockWorld, IDisposable
{
    private readonly List<Chunk> _chunksToRemove = new();
    public readonly ChunkStatusManager ChunkStatusManager;
    public readonly WorldChunkStorage ChunkStorage = new();
    public readonly IWorldGenerator Generator;
    public readonly LogicProcessor LogicProcessor;
    public readonly Random Random = new();
    public readonly WorldSaveManager? SaveManager;

    public World(LogicProcessor logicProcessor, string saveLocation)
    {
        if (logicProcessor.WorldData.WorldSettings.Flat)
            Generator = new FlatWorldGenerator();
        else
            Generator = new WorldGenerator(logicProcessor.WorldData.WorldSettings.Seed);

        LogicProcessor = logicProcessor;
        if (logicProcessor.LogicalSide != LogicalSide.Client)
        {
            SaveManager = new WorldSaveManager(saveLocation);
            SaveManager.CreateDirectory();
        }

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

    public bool IsBlockLoaded(Vector3D<int> pos)
    {
        var chunkPos = pos.ShiftRight(Constants.ChunkSizeLog2);
        var c = GetChunk(chunkPos, false);
        return c is { ChunkStatusInfo.ReadyForUse: true };
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

    public void AddChunk(Chunk chunk)
    {
        ChunkStorage.AddChunk(chunk);
    }

    public void RemoveChunk(Vector3D<int> pos)
    {
        var c = ChunkStorage.GetChunk(pos)!;
        c.ChunkStatusInfo.LoadTask?.Wait();
        if (c.ChunkStatusInfo.ReadyForUse) ChunkStatusManager.OnChunkNotReadyForUse(c);

        ChunkStorage.RemoveChunk(pos);
        if (c.Region != null) --c.Region.DependencyCount;
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return ChunkStorage.GetLoadedChunks();
    }

    public void Dispose()
    {
        _chunksToRemove.AddRange(GetLoadedChunks());

        foreach (var chunk in _chunksToRemove) RemoveChunk(chunk.Position);

        _chunksToRemove.Clear();
        if (LogicProcessor.LogicalSide != LogicalSide.Client) SaveManager!.Dispose();
    }

    private Chunk BeginLoadingChunk(Vector3D<int> pos)
    {
        if (LogicProcessor.LogicalSide == LogicalSide.Client)
            throw new InvalidOperationException("Can't load chunks on client");
        var region = SaveManager!.GetRegion(pos.ShiftRight(ChunkRegion.SizeLog2));
        var nc = new Chunk(this, pos, region);
        ++region.DependencyCount;
        nc.ChunkStatusInfo.StartLoadTask();
        AddChunk(nc);
        return nc;
    }

    public void Update()
    {
        ChunkStatusManager.Update();
        _chunksToRemove.Clear();
        if (LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            SaveManager!.Update();
        }
    }
}