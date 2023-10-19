using BlockFactory.Entity_;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class Chunk : IBlockStorage
{
    public delegate void BlockEventHandler(Vector3D<int> pos);

    public readonly ChunkNeighbourhood Neighbourhood;
    public readonly Vector3D<int> Position;
    public ChunkData? Data;
    public bool ReadyForTick = false;
    public bool ReadyForUse = false;
    public int ReadyForUseNeighbours = 0;
    public HashSet<PlayerEntity> WatchingPlayers = new();

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

    public event BlockEventHandler BlockUpdate = p => { };

    public void AddWatchingPlayer(PlayerEntity player)
    {
        WatchingPlayers.Add(player);
    }

    public void RemoveWatchingPlayer(PlayerEntity player)
    {
        WatchingPlayers.Remove(player);
    }
}