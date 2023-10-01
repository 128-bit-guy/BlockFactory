using Silk.NET.Maths;

namespace BlockFactory.World_;

public class Chunk : IBlockStorage
{
    public ChunkData? Data;
    public readonly Vector3D<int> Position;
    public readonly ChunkNeighbourhood Neighbourhood;

    public Chunk(Vector3D<int> position)
    {
        Position = position;
        Neighbourhood = new ChunkNeighbourhood(this);
    }

    public short GetBlock(Vector3D<int> pos)
    {
        return Data!.GetBlock(pos);
    }

    public void SetBlock(Vector3D<int> pos, short block)
    {
        Data!.SetBlock(pos, block);
    }
}