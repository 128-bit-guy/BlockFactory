using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Math_;
using BlockFactory.Network.Packet_;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using BlockFactory.World_.Serialization;
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
    private bool _loadingCompleted;
    public bool ReadyForTick = false;
    public bool ReadyForUse = false;
    public bool IsValid = false;
    public bool IsTicking = false;
    private int _tickingDependencies = 0;
    public int ReadyForUseNeighbours = 0;
    public readonly HashSet<PlayerEntity> WatchingPlayers = new();
    public readonly ChunkRegion? Region;
    public readonly List<Vector3D<int>> ScheduledLightUpdates = new();

    public Chunk(World world, Vector3D<int> position, ChunkRegion? region)
    {
        Position = position;
        Region = region;
        World = world;
        Neighbourhood = new ChunkNeighbourhood(this);
    }

    public bool IsLoaded => (Data != null && LoadTask == null) || (LoadTask!.IsCompleted || _loadingCompleted);

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

    public byte GetLight(Vector3D<int> pos, LightChannel channel)
    {
        return Data!.GetLight(pos, channel);
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        LoadTask?.Wait();
        if (Data!.GetBlock(pos) == block) return;
        Data!.SetBlock(pos, block, update);
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
        {
            foreach (var player in WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, new BlockChangePacket(pos, block));
            }
        }

        if (!update) return;
        // if (World.LogicProcessor.LogicalSide == LogicalSide.Client)
        // {
        //     
        //     return;
        // }
        ScheduleLightUpdate(pos);
        UpdateBlock(pos);
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            var oPos = pos + new Vector3D<int>(i, j, k);
            var oc = Neighbourhood.GetChunk(oPos.ShiftRight(Constants.ChunkSizeLog2), false);
            if (World.LogicProcessor.LogicalSide == LogicalSide.Client && oc == null) continue;
            Neighbourhood.UpdateBlock(oPos);
        }
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        LoadTask?.Wait();
        Data!.SetBiome(pos, biome);
    }

    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light)
    {
        if (Data!.GetLight(pos, channel) == light) return;
        Data!.SetLight(pos, channel, light);
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
        {
            foreach (var player in WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, new LightChangePacket(pos, channel, light));
            }
        }

        Neighbourhood.UpdateLight(pos);
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
        if (World.LogicProcessor.LogicalSide != LogicalSide.Client && GetBlock(pos) == 4 &&
            Neighbourhood.GetBlock(pos + Vector3D<int>.UnitY) != 0) SetBlock(pos, 3);
        BlockUpdate(pos);
    }

    public void UpdateLight(Vector3D<int> pos)
    {
        LightUpdate(pos);
    }

    public void ScheduleLightUpdate(Vector3D<int> pos)
    {
        if (!Data!.IsLightUpdateScheduled(pos))
        {
            Data!.SetLightUpdateScheduled(pos, true);
            ScheduledLightUpdates.Add(pos);
        }
    }

    public event BlockEventHandler BlockUpdate = p => { };
    public event BlockEventHandler LightUpdate = p => { };

    public void AddWatchingPlayer(PlayerEntity player)
    {
        WatchingPlayers.Add(player);
    }

    public void RemoveWatchingPlayer(PlayerEntity player)
    {
        WatchingPlayers.Remove(player);
        World.ChunkStatusManager.ScheduleStatusUpdate(this);
    }

    public void Update(bool heavy)
    {
        if (World.LogicProcessor.LogicalSide == LogicalSide.Client) return;
        bool wasInitialized = Data!.Decorated && Data!.HasSkyLight;
        if (heavy && !Data!.Decorated)
        {
            Data!.Decorated = true;
            World.Generator.DecorateChunk(this);
        }

        if (!Data!.Decorated) return;

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

        if (heavy || Data!.HasSkyLight)
        {
            LightPropagator.ProcessLightUpdates(this);
        }

        if (wasInitialized) return;
        var isInitialized = Data!.Decorated && Data!.HasSkyLight;
        if (!isInitialized) return;

        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            ++Neighbourhood.GetChunk(Position + new Vector3D<int>(i, j, k))!.Data!.DecoratedNeighbours;
        }
    }

    private void CopyLightUpdatesFromData()
    {
        ScheduledLightUpdates.Clear();
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var absPos = new Vector3D<int>(i, j, k) + Position.ShiftLeft(Constants.ChunkSizeLog2);
            if (Data!.IsLightUpdateScheduled(absPos))
            {
                ScheduledLightUpdates.Add(absPos);
            }
        }
    }

    private void GenerateOrLoad()
    {
        var data = Region!.GetChunk(Position);
        if (data == null)
        {
            World.Generator.GenerateChunk(this);
            Region.SetChunk(Position, Data!);
        }
        else
        {
            Data = data;
        }

        CopyLightUpdatesFromData();

        _loadingCompleted = true;

        World.ChunkStatusManager.ScheduleStatusUpdate(this);
    }

    public void StartLoadTask()
    {
        if (Region!.LoadTask == null)
        {
            LoadTask = Task.Run(GenerateOrLoad);
        }
        else
        {
            LoadTask = Task.Factory.ContinueWhenAll(new Task[] { Region.LoadTask }, _ => GenerateOrLoad());
        }
    }

    public void AddTickingDependency()
    {
        ++_tickingDependencies;
        World.ChunkStatusManager.ScheduleTickingUpdate(this);
    }

    public void RemoveTickingDependency()
    {
        --_tickingDependencies;
    }

    public int GetUpdateClass()
    {
        var x = Position.X % 3;
        if (x < 0) x += 3;
        var y = Position.Y % 3;
        if (y < 0) y += 3;
        var z = Position.Z % 3;
        if (z < 0) z += 3;
        return x + y * 3 + z * 9;
    }

    public bool ShouldTick()
    {
        if (!IsValid || !ReadyForTick) return false;
        return !Data!.Decorated || !Data!.HasSkyLight || _tickingDependencies > 0;
    }
}