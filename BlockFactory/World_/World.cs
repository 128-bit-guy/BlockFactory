using BlockFactory.Base;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class World : IChunkStorage
{
    public readonly ChunkStatusManager ChunkStatusManager;
    public readonly WorldChunkStorage ChunkStorage = new();
    public readonly WorldGenerator Generator = new();

    public World()
    {
        ChunkStatusManager = new ChunkStatusManager(this);
    }

    public short GetBlock(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBlock(pos);
    }

    public void SetBlock(Vector3D<int> pos, short block)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBlock(pos, block);
    }

    public Chunk? GetChunk(Vector3D<int> pos, bool load = true)
    {
        var res = ChunkStorage.GetChunk(pos, false);
        if (res != null || !load) return res;
        var nc = new Chunk(pos)
        {
            Data = new ChunkData()
        };
        Generator.GenerateChunk(nc);
        AddChunk(nc);
        return nc;
    }

    public void AddChunk(Chunk chunk)
    {
        ChunkStorage.AddChunk(chunk);
        ChunkStatusManager.OnChunkReadyForUse(chunk);
    }

    public void RemoveChunk(Vector3D<int> pos)
    {
        var c = ChunkStorage.GetChunk(pos)!;
        ChunkStatusManager.OnChunkNotReadyForUse(c);
        ChunkStorage.RemoveChunk(pos);
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return ChunkStorage.GetLoadedChunks();
    }
}