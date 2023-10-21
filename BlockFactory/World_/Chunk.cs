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
    public readonly ChunkRegion Region;

    public Chunk(World world, Vector3D<int> position, ChunkRegion region)
    {
        Position = position;
        Region = region;
        World = world;
        Neighbourhood = new ChunkNeighbourhood(this);
    }

    public bool IsLoaded => (Data != null && LoadTask == null) || LoadTask!.IsCompleted;

    public short GetBlock(Vector3D<int> pos)
    {
        LoadTask?.Wait();
        return Data!.GetBlock(pos);
    }

    public byte GetBiome(Vector3D<int> pos)
    {
        LoadTask?.Wait();
        return Data!.GetBiome(pos);
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

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        LoadTask?.Wait();
        Data!.SetBiome(pos, biome);
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

    private void GenerateOrLoad()
    {
        var data = Region.GetChunk(Position);
        if (data == null)
        {
            World.Generator.GenerateChunk(this);
            Region.SetChunk(Position, Data!);
        }
        else
        {
            Data = data;
        }
    }

    public void StartLoadTask()
    {
        if (Region.LoadTask == null)
        {
            LoadTask = Task.Run(GenerateOrLoad);
        }
        else
        {
            LoadTask = Task.Factory.ContinueWhenAll(new Task[] { Region.LoadTask }, _ => GenerateOrLoad());
        }
    }
}