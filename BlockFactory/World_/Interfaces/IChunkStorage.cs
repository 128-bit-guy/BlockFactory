using Silk.NET.Maths;

namespace BlockFactory.World_.Interfaces;

public interface IChunkStorage
{
    public Chunk? GetChunk(Vector3D<int> pos, bool load = true);
    public void AddChunk(Chunk chunk);
    public void RemoveChunk(Vector3D<int> pos);

    public IEnumerable<Chunk> GetLoadedChunks();
}