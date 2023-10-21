using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class Chunk : IBlockWorld
{
    public delegate void BlockEventHandler(Vector3D<int> pos);

    public readonly ChunkNeighbourhood Neighbourhood;
    public readonly Vector3D<int> Position;

    public readonly World World;
    public ChunkData? Data;
    public Task? LoadTask = null;
    public bool ReadyForTick = false;
    public bool ReadyForUse = false;
    public int ReadyForUseNeighbours = 0;
    public HashSet<PlayerEntity> WatchingPlayers = new();

    public Chunk(World world, Vector3D<int> position)
    {
        Position = position;
        World = world;
        Neighbourhood = new ChunkNeighbourhood(this);
    }

    public bool IsLoaded => (Data != null && LoadTask == null) || LoadTask!.IsCompleted;

    public short GetBlock(Vector3D<int> pos)
    {
        LoadTask?.Wait();
        return Data!.GetBlock(pos);
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        LoadTask?.Wait();
        Data!.SetBlock(pos, block, update);
        if (!update) return;
        UpdateBlock(pos);
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
        if (GetBlock(pos) == 4 && Neighbourhood.GetBlock(pos + Vector3D<int>.UnitY) != 0) SetBlock(pos, 3);
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

    public void Update()
    {
        if (!Data!.Decorated)
        {
            Data!.Decorated = true;
            World.Generator.DecorateChunk(this);
        }

        var x = World.Random.Next(Constants.ChunkSize);
        var y = World.Random.Next(Constants.ChunkSize);
        var z = World.Random.Next(Constants.ChunkSize);
        var absPos = new Vector3D<int>(x, y, z) + Position.ShiftLeft(Constants.ChunkSizeLog2);
        if (GetBlock(absPos) == 3 && GetBlock(absPos + Vector3D<int>.UnitY) == 0)
        {
            for (var i = -1; i <= 1; ++i)
            for (var j = -1; j <= 1; ++j)
            for (var k = -1; k <= 1; ++k)
            {
                var oPos = absPos + new Vector3D<int>(i, j, k);
                if (Neighbourhood.GetBlock(oPos) != 4) continue;
                SetBlock(absPos, 4);
                goto EndLoop;
            }

            EndLoop: ;
        }
    }
}