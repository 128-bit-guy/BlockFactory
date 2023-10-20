using BlockFactory.Entity_;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class Chunk : IBlockWorld
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

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        Data!.SetBlock(pos, block, update);
        if(!update) return;
        BlockUpdate(pos);
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            var oPos = pos + new Vector3D<int>(i, j, k);
            Neighbourhood.UpdateBlock(oPos);
        }
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
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