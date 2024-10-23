using System.Collections;
using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Content.Entity_;
using BlockFactory.Network.Packet_;
using BlockFactory.Utils;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class Chunk : IBlockWorld, IEntityStorage
{
    public readonly ChunkNeighbourhood Neighbourhood;
    
    public readonly Vector3D<int> Position;
    public readonly ChunkRegion? Region;
    public readonly World World;
    public ChunkData? Data;
    [ThreadStatic] private static List<Entity>? _entitiesToMove;
    [ThreadStatic] private static List<Entity>? _entitiesToUpdate;
    
    public readonly ChunkStatusInfo ChunkStatusInfo;
    public readonly ChunkUpdateInfo ChunkUpdateInfo;


    public Chunk(World world, Vector3D<int> position, ChunkRegion? region)
    {
        Position = position;
        Region = region;
        World = world;
        Neighbourhood = new ChunkNeighbourhood(this);
        ChunkStatusInfo = new ChunkStatusInfo(this);
        ChunkUpdateInfo = new ChunkUpdateInfo(this);
    }

    public short GetBlock(Vector3D<int> pos)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        return Data!.GetBlock(pos);
    }

    public byte GetBiome(Vector3D<int> pos)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        return Data!.GetBiome(pos);
    }

    public byte GetLight(Vector3D<int> pos, LightChannel channel)
    {
        return Data!.GetLight(pos, channel);
    }

    public bool IsBlockLoaded(Vector3D<int> pos)
    {
        return pos.ShiftRight(Constants.ChunkSizeLog2) == Position;
    }

    public float GetDayCoefficient()
    {
        return World.GetDayCoefficient();
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        if (Data!.GetBlock(pos) == block) return;
        Data!.SetBlock(pos, block, update);
        ChunkUpdateInfo.OnBlockChanged(pos);
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, new BlockChangePacket(pos, block));
            }

        if (!update) return;
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
        ChunkStatusInfo.LoadTask?.Wait();
        Data!.SetBiome(pos, biome);
    }

    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light)
    {
        if (Data!.GetLight(pos, channel) == light) return;
        Data!.SetLight(pos, channel, light);
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, new LightChangePacket(pos, channel, light));
            }

        Neighbourhood.UpdateLight(pos);
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
        ChunkUpdateInfo.UpdateBlock(pos);
    }

    public void ScheduleLightUpdate(Vector3D<int> pos)
    {
        ChunkUpdateInfo.ScheduleLightUpdate(pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & Constants.ChunkMask) | (((pos.Y & Constants.ChunkMask) | ((pos.Z & Constants.ChunkMask)
            << Constants.ChunkSizeLog2)) << Constants.ChunkSizeLog2);
    }

    public void MoveEntityToChunk(Entity entity, Chunk c)
    {
        RemoveEntityInternal(entity, false, false);
        Data!.RemoveEntity(entity);
        c.Data!.AddEntity(entity);
        c.AddEntityInternal(entity, false, false);
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
        {
            EntityChangeChunkPacket? movePacket = null;
            AddEntityPacket? addPacket = null;
            RemoveEntityPacket? removePacket = null;
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                if (player.ChunkLoader!.IsChunkVisible(c))
                {
                    movePacket ??= new EntityChangeChunkPacket(entity.Guid, Position, c.Position);
                    World.LogicProcessor.NetworkHandler.SendPacket(player, movePacket);
                }
                else
                {
                    removePacket ??= new RemoveEntityPacket(Position, entity.Guid);
                    World.LogicProcessor.NetworkHandler.SendPacket(player, removePacket);
                }
            }

            foreach (var player in c.ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(c) || player.ChunkLoader.IsChunkVisible(this)) continue;
                addPacket ??= new AddEntityPacket(c.Position, entity);
                World.LogicProcessor.NetworkHandler.SendPacket(player, addPacket);
            }
        }
    }

    private void FullyDecoratedUpdate()
    {
        var x = World.Random.Next(Constants.ChunkSize);
        var y = World.Random.Next(Constants.ChunkSize);
        var z = World.Random.Next(Constants.ChunkSize);
        var absPos = new Vector3D<int>(x, y, z) + Position.ShiftLeft(Constants.ChunkSizeLog2);
        this.GetBlockObj(absPos).RandomUpdateBlock(new BlockPointer(Neighbourhood, absPos));
        ChunkUpdateInfo.UpdateBlocksInBuffer();
        _entitiesToMove ??= new List<Entity>();
        foreach (var (_, entity) in Data!.Entities)
        {
            if (entity.GetChunkPos() != Position)
            {
                _entitiesToMove.Add(entity);
            }
        }

        foreach (var entity in _entitiesToMove)
        {
            var pos = entity.GetChunkPos();
            var c = Neighbourhood.GetChunk(pos, false);
            if (c != null)
            {
                MoveEntityToChunk(entity, c);
            }
            else
            {
                RemoveEntityInternal(entity, false);
                Data!.RemoveEntity(entity);
                World.PendingEntityManager.AddEntity(entity);
            }
        }
        _entitiesToMove.Clear();
    }

    public void PreUpdate()
    {
        if (!Data!.FullyDecorated) return;
        ChunkUpdateInfo.MoveBlockUpdatesToBuffer();
        
        _entitiesToMove ??= new List<Entity>();
        _entitiesToUpdate ??= new List<Entity>();
        _entitiesToUpdate.AddRange(Data!.Entities.Values);
        
        foreach (var entity in _entitiesToUpdate)
        {
            entity.Update();
            if (World.LogicProcessor.LogicalSide == LogicalSide.Server &&
                (entity.Pos - entity.LastSentPos).LengthSquared > 1e-5)
            {
                _entitiesToMove.Add(entity);
            }
        }
        
        _entitiesToUpdate.Clear();

        if (World.LogicProcessor.LogicalSide == LogicalSide.Server && _entitiesToMove.Count > 0)
        {
            var list = new List<(Guid, Vector3D<double>)>();
            foreach (var entity in _entitiesToMove)
            {
                entity.LastSentPos = entity.Pos;
                list.Add((entity.Guid, entity.Pos));
            }

            var packet = new EntityPosUpdatePacket(Position, list);
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, packet);
            }
            _entitiesToMove.Clear();
        }
    }

    public void Update(bool heavy)
    {
        if (World.LogicProcessor.LogicalSide == LogicalSide.Client) return;
        var wasInitialized = Data!.Decorated && Data!.HasSkyLight;
        if (heavy && !Data!.Decorated)
        {
            Data!.Decorated = true;
            World.Generator.DecorateChunk(this);
        }

        if (!Data!.Decorated) return;

        if (Data!.FullyDecorated) FullyDecoratedUpdate();

        if (heavy || Data!.HasSkyLight) LightPropagator.ProcessLightUpdates(this);

        if (wasInitialized) return;
        var isInitialized = Data!.Decorated && Data!.HasSkyLight;
        if (!isInitialized) return;

        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
            ++Neighbourhood.GetChunk(Position + new Vector3D<int>(i, j, k))!.Data!.DecoratedNeighbours;
    }

    public void GenerateOrLoad()
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

        OnLoaded(data != null);

        ChunkStatusInfo.SetLoadingCompleted();

        World.ChunkStatusManager.ScheduleStatusUpdate(this);
    }

    public void OnLoaded(bool serialization)
    {
        ChunkUpdateInfo.CopyUpdatesFromData();
        foreach (var entity in Data!.Entities.Values)
        {
            World.AddEntityInternal(entity, serialization);
            AddEntityInternal(entity, serialization);
        }
    }

    public void OnUnloaded()
    {
        foreach (var entity in Data!.Entities.Values)
        {
            RemoveEntityInternal(entity, true);
            World.RemoveEntityInternal(entity, true);
        }
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

    public Entity? GetEntity(Guid guid)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        return Data!.GetEntity(guid);
    }

    public IEnumerable<Entity> GetEntities(Box3D<double> box)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        return Data!.GetEntities(box);
    }

    public void RemoveEntityInternal(Entity entity, bool serialization, bool sendPacket = true)
    {
        if (entity.Chunk != this)
        {
            throw new ArgumentException("Entity is not added to this chunk", nameof(entity));
        }
        if (sendPacket && World.LogicProcessor.LogicalSide == LogicalSide.Server)
        {
            var packet = new RemoveEntityPacket(Position, entity.Guid);
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, packet);
            }
        }
        entity.SetChunk(null, serialization);
    }

    public void AddEntityInternal(Entity entity, bool serialization, bool sendPacket = true)
    {
        if (entity.Chunk != null)
        {
            throw new ArgumentException("Entity is already added to a chunk", nameof(entity));
        }
        entity.SetChunk(this, serialization);
        if (sendPacket && World.LogicProcessor.LogicalSide == LogicalSide.Server)
        {
            var packet = new AddEntityPacket(Position, entity);
            foreach (var player in ChunkStatusInfo.WatchingPlayers)
            {
                if (!player.ChunkLoader!.IsChunkVisible(this)) continue;
                World.LogicProcessor.NetworkHandler.SendPacket(player, packet);
            }
        }
    }

    public void AddEntity(Entity entity)
    {
        ChunkStatusInfo.LoadTask?.Wait();
        World.AddEntityInternal(entity, false);
        Data!.AddEntity(entity);
        AddEntityInternal(entity, false);
    }

    public void RemoveEntity(Entity entity)
    {
        RemoveEntityInternal(entity, false);
        Data!.RemoveEntity(entity);
        World.RemoveEntityInternal(entity, false);
    }
}