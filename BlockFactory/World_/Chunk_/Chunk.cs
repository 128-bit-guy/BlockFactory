using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Side_;
using BlockFactory.Util.Dependency;
using BlockFactory.World_.Api;
using BlockFactory.World_.Save;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Chunk_;

public class Chunk : IBlockStorage, IEntityStorage, IDependable
{
    public readonly Vector3i Pos;
    public readonly ChunkRegion? Region;
    public readonly World World;
    private bool _chunkDataCreated;
    private ChunkData? _data;
    private int _dependencyCount;
    public bool ExistsInWorld;

    public Task? GenerationTask;
    public ChunkNeighbourhood Neighbourhood;
    public Dictionary<long, PlayerEntity> ViewingPlayers = new();

    [ExclusiveTo(Side.Client)]
    public Chunk(ChunkData data, Vector3i pos, World world) : this(pos, world, null)
    {
        _data = data;
    }

    public Chunk(Vector3i pos, World world, ChunkRegion? region)
    {
        Pos = pos;
        World = world;
        _dependencyCount = 0;
        Neighbourhood = new ChunkNeighbourhood(this);
        Region = region;
    }

    public ChunkData Data
    {
        get { return _data ??= Region!.GetOrCreateChunkData(Pos.BitAnd(ChunkRegion.Mask), out _chunkDataCreated); }
    }

    public bool Generated => GenerationTask == null || GenerationTask.IsCompleted;

    public BlockState GetBlockState(Vector3i pos)
    {
        if (IsInside(pos))
        {
            var beg = GetBegin();
            var cur = pos - beg;
            return Data!.GetBlockState(cur);
        }

        return World.GetBlockState(pos);
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        if (IsInside(pos))
        {
            var beg = GetBegin();
            var cur = pos - beg;
            var prevState = Data!.GetBlockState(cur);
            if (prevState != state)
            {
                Data.SetBlockState(cur, state);
                foreach (var (_, player) in ViewingPlayers) player.VisibleBlockChanged(this, pos, prevState, state);
            }
        }
        else
        {
            World.SetBlockState(pos, state);
        }
    }

    public ref int DependencyCount => ref _dependencyCount;

    public void RunGenerationTask()
    {
        GenerationTask = Region!.LoadTask == null
            ? Task.Run(Generate)
            : Task.Factory.ContinueWhenAll(new[] { Region.LoadTask }, _ => Generate());
    }

    public void EnsureGenerated()
    {
        if (GenerationTask == null) return;
        GenerationTask.Wait();
        GenerationTask = null;
    }

    private void Generate()
    {
        var d = Data;
        if (_chunkDataCreated) World.Generator.GenerateBaseSurface(this);
        foreach (var entity in Data.EntitiesInChunk.Values)
        {
            entity.Chunk = this;
            World.OnLoadedEntityAdded(entity);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3i GetBegin()
    {
        return Pos.BitShiftLeft(Constants.ChunkSizeLog2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(Vector3i pos)
    {
        var beg = GetBegin();
        var cur = pos - beg;
        for (var i = 0; i < 3; ++i)
            if (cur[i] < 0 || cur[i] >= Constants.ChunkSize)
                return false;
        return true;
    }

    public Box3i GetInclusiveBox()
    {
        var begin = GetBegin();
        return new Box3i(begin, begin + new Vector3i(Constants.ChunkSize - 1));
    }

    public void AddEntity(Entity entity, bool loaded = false)
    {
        if (entity.Pos.ChunkPos == Pos)
        {
            entity.Chunk = this;
            World.OnEntityAdded(entity, loaded);
            Data.EntitiesInChunk.Add(entity.Id, entity);
            if (!World.GameInstance.Kind.IsNetworked() || !World.GameInstance.Kind.DoesProcessLogic()) return;
            foreach (var player in ViewingPlayers.Values.Where(player => entity != player))
            {
                World.GameInstance.NetworkHandler.GetPlayerConnection(player)
                    .SendPacket(new EntityAddedPacket(entity.Type, ((ITagSerializable)entity).SerializeToTag()));
            }
        }
        else
        {
            World.AddEntity(entity, loaded);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (entity.Pos.ChunkPos == Pos)
        {
            entity.Chunk = null;
            World.OnEntityRemoved(entity);
            Data.EntitiesInChunk.Remove(entity.Id);
            if (!World.GameInstance.Kind.IsNetworked() || !World.GameInstance.Kind.DoesProcessLogic()) return;
            foreach (var player in ViewingPlayers.Values.Where(player => entity != player))
            {
                World.GameInstance.NetworkHandler.GetPlayerConnection(player)
                    .SendPacket(new EntityRemovedPacket(Pos, entity.Id));
            }
        }
        else
        {
            World.RemoveEntity(entity);
        }
    }

    public void OnRemovedFromWorld()
    {
        foreach (var entity in Data.EntitiesInChunk.Values)
        {
            entity.Chunk = null;
            World.OnEntityRemoved(entity);
        }
    }

    public Entity GetEntity(long id)
    {
        return Data.EntitiesInChunk[id];
    }

    public void TickPass0()
    {
        if(!Neighbourhood.AreAllNeighboursDecorated()) return;
        var list = new List<Entity>(Data.EntitiesInChunk.Values);
        foreach (var entity in list)
        {
            entity.Tick();
        }
    }

    public void TickPass1()
    {
        if(!Neighbourhood.AreAllNeighboursDecorated()) return;
        var idsToRemove = new List<long>();
        foreach (var entity in Data.EntitiesInChunk.Values)
        {
            entity.Pos.Fix();
            if (entity.Pos.ChunkPos == Pos) continue;
            idsToRemove.Add(entity.Id);
            var c = Neighbourhood.GetChunk(entity.Pos.ChunkPos);
            c.Data.EntitiesInChunk.Add(entity.Id, entity);
            entity.Chunk = c;
        }

        foreach (var i in idsToRemove)
        {
            Data.EntitiesInChunk.Remove(i);
        }
    }
}