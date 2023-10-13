using Silk.NET.Maths;

namespace BlockFactory.World_;

public class Chunk : IBlockStorage
{
    public readonly ChunkNeighbourhood Neighbourhood;
    public readonly Vector3D<int> Position;
    public ChunkData? Data;
    public int ReadyForUseNeighbours = 0;
    public bool ReadyForUse = false;
    public bool ReadyForTick = false;

    public delegate void BlockEventHandler(Vector3D<int> pos);

    public event BlockEventHandler BlockUpdate = p => { };

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
        BlockUpdate(pos);
    }
}