using BlockFactory.Base;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class World : IChunkStorage, IBlockWorld
{
    public readonly ChunkStatusManager ChunkStatusManager;
    public readonly WorldChunkStorage ChunkStorage = new();
    public readonly WorldGenerator Generator = new();
    private List<Chunk> _chunksToRemove = new();
    public readonly Random Random = new Random();

    public World()
    {
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

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBlock(pos, block, update);
    }

    public Chunk? GetChunk(Vector3D<int> pos, bool load = true)
    {
        var res = ChunkStorage.GetChunk(pos, false);
        if (res != null || !load) return res;
        var nc = new Chunk(this, pos)
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

    public void Update()
    {
        // var chunks = GetLoadedChunks().ToList();
        foreach (var chunk in GetLoadedChunks())
        {
            if (chunk.WatchingPlayers.Count == 0)
            {
                _chunksToRemove.Add(chunk);
            }
            else if(chunk.ReadyForTick)
            {
                chunk.Update();
            }
        }

        foreach (var chunk in _chunksToRemove)
        {
            RemoveChunk(chunk.Position);
        }
        _chunksToRemove.Clear();
    }
}