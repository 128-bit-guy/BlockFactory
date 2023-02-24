using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Side_;
using BlockFactory.Util;
using BlockFactory.Util.Dependency;
using BlockFactory.World_.Api;
using BlockFactory.World_.Chunk_;
using BlockFactory.World_.Gen;
using BlockFactory.World_.Save;
using OpenTK.Mathematics;

namespace BlockFactory.World_;

public class World : IBlockStorage, IDisposable, IEntityStorage
{
    public delegate void ChunkEventHandler(Chunk chunk);

    private readonly Dictionary<Vector3i, Chunk> _chunks = new();
    private readonly List<Chunk>[] _groupedChunks;
    private readonly Dictionary<long, PlayerEntity> _players = new();
    public readonly GameInstance GameInstance;
    public readonly WorldGenerator Generator = null!;
    public readonly WorldSaveManager SaveManager = null!;
    private bool _decoratingChunks;
    private long _gameTime;
    private long _lastId;

    public World(GameInstance gameInstance, int seed, string saveName)
    {
        GameInstance = gameInstance;
        if (GameInstance.Kind.DoesProcessLogic())
        {
            Generator = new WorldGenerator(seed);
        }

        OnChunkAdded += OnChunkAdded0;
        OnChunkRemoved += OnChunkRemoved0;
        if (GameInstance.Kind.DoesProcessLogic())
        {
            SaveManager = new WorldSaveManager(this, saveName);
        }

        _groupedChunks = new List<Chunk>[3 * 3 * 3];
        foreach (var pos in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
            _groupedChunks[pos.X * 9 + pos.Y * 3 + pos.Z] = new List<Chunk>();

        _decoratingChunks = false;

        _gameTime = 0;
    }

    public BlockState GetBlockState(Vector3i pos)
    {
        return GetOrLoadGeneratedChunk(pos.BitShiftRight(Constants.ChunkSizeLog2)).GetBlockState(pos);
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        GetOrLoadGeneratedChunk(pos.BitShiftRight(Constants.ChunkSizeLog2)).SetBlockState(pos, state);
    }

    public void Dispose()
    {
        UnloadChunks(true);
        if (GameInstance.Kind.DoesProcessLogic())
        {
            SaveManager.Dispose();
        }
    }

    public event ChunkEventHandler OnChunkAdded = _ => { };
    public event ChunkEventHandler OnChunkRemoved = _ => { };

    public void OnEntityAdded(Entity entity, bool loaded)
    {
        if (loaded)
        {
            OnLoadedEntityAdded(entity);
        }
        else
        {
            OnNewEntityAdded(entity);
        }
    }

    public void OnNewEntityAdded(Entity entity)
    {
        entity.Id = Interlocked.Increment(ref _lastId);
        OnLoadedEntityAdded(entity);
    }

    public void OnLoadedEntityAdded(Entity entity)
    {
        entity.GameInstance = GameInstance;
        entity.World = this;
        entity.OnAddToWorld();
        if (entity is PlayerEntity playerEntity && GameInstance.Kind.DoesProcessLogic())
        {
            _players[playerEntity.Id] = playerEntity;
        }
    }

    public void OnEntityRemoved(Entity entity)
    {
        entity.OnRemoveFromWorld();
        if (entity is PlayerEntity && GameInstance.Kind.DoesProcessLogic())
        {
            _players.Remove(entity.Id);
        }
    }

    [ExclusiveTo(Side.Client)]
    private Chunk CreateFakePlayerChunk(Vector3i pos)
    {
        var c = new Chunk(pos, this);
        _chunks.Add(pos, c);
        return c;
    }

    public Chunk GetOrLoadChunk(Vector3i pos)
    {
        if (Thread.CurrentThread != GameInstance.MainThread)
            throw new InvalidOperationException("Can not get chunk from not main thread!");

        if (_decoratingChunks) throw new InvalidOperationException("Can not get chunk when decorating chunks!");
        if (_chunks.TryGetValue(pos, out var ch)) return ch;
        if (!GameInstance.Kind.DoesProcessLogic())
        {
            return CreateFakePlayerChunk(pos);
        }

        var regionPos = pos.BitShiftRight(ChunkRegion.SizeLog2);
        var region = SaveManager.GetRegion(regionPos);
        region.EnsureLoading();
        ((IDependable)region).OnDependencyAdded();
        var chunk = _chunks[pos] = new Chunk(pos, this, region);
        OnChunkAdded(chunk);
        chunk.RunGenerationTask();

        return chunk;
    }

    public Chunk GetOrLoadGeneratedChunk(Vector3i pos)
    {
        var ch = GetOrLoadChunk(pos);
        ch.EnsureGenerated();
        return ch;
    }

    private void DecorateChunkIfNecessary(Chunk c)
    {
        if (!c.ExistsInWorld || !c.Neighbourhood.AreAllNeighboursLoaded() || c.Data.Decorated) return;

        c.Data.Decorated = true;
        Generator.Decorate(c);
    }

    public void Tick()
    {
        foreach (var list in _groupedChunks)
        {
            Parallel.ForEach(list, c => c.TickPass0());
        }

        foreach (var list in _groupedChunks)
        {
            Parallel.ForEach(list, c => c.TickPass1());
        }
        // foreach (var (id, player) in _players) player.Tick();

        if (GameInstance.Kind.DoesProcessLogic())
        {
            foreach (var (id, player) in _players) player.LoadChunks();

            foreach (var (id, player) in _players)
            {
                player.ProcessScheduledAddedVisibleChunks();
                player.UnloadChunks(false);
            }

            _decoratingChunks = true;

            Parallel.ForEach(_groupedChunks[_gameTime % 27], DecorateChunkIfNecessary);

            _decoratingChunks = false;

            UnloadChunks(false);
            SaveManager.UnloadRegions();
            ++_gameTime;
        }
        else
        {
            FixGroupedChunks();
        }
    }

    private void UnloadChunks(bool all)
    {
        var posesToRemove = new List<(Vector3i, Chunk)>();
        foreach (var (pos, chunk) in _chunks)
        {
            if (chunk is not { DependencyCount: 0, ViewingPlayers.Count: 0, Generated: true } && !all) continue;
            chunk.EnsureGenerated();
            posesToRemove.Add((pos, chunk));
        }

        foreach (var (pos, chunk) in posesToRemove)
        {
            // Console.WriteLine($"Removing chunk at {pos}");
            OnChunkRemoved(chunk);
            if (GameInstance.Kind.DoesProcessLogic())
            {
                var regionPos = pos.BitShiftRight(ChunkRegion.SizeLog2);
                var region = SaveManager.GetRegion(regionPos);
                ((IDependable)region).OnDependencyRemoved();
            }

            _chunks.Remove(pos);
        }

        FixGroupedChunks();
    }

    private void FixGroupedChunks()
    {
        foreach (var pos in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
            _groupedChunks[pos.X * 9 + pos.Y * 3 + pos.Z].RemoveIf(c => !c.ExistsInWorld);
    }

    [ExclusiveTo(Side.Client)]
    public void AddChunk(Chunk chunk)
    {
        if (_chunks.TryGetValue(chunk.Pos, out var c))
        {
            if (!c.IsFakePlayerChunk) 
                throw new InvalidOperationException($"Chunk at pos {chunk.Pos} is already added");
        }

        _chunks[chunk.Pos] = chunk;
        OnChunkAdded(chunk);
        chunk.AddObjectsToWorld();
        if (c != null)
        {
            foreach (var entity in c.Data.EntitiesInChunk.Values)
            {
                chunk.Data.EntitiesInChunk.Add(entity.Id, entity);
                entity.Chunk = chunk;
            }
        }
    }

    [ExclusiveTo(Side.Client)]
    public void RemoveChunk(Vector3i chunkPos)
    {
        var ch = _chunks[chunkPos];
        OnChunkRemoved(ch);
        _chunks.Remove(chunkPos);
    }

    private void OnChunkAdded0(Chunk chunk)
    {
        chunk.ExistsInWorld = true;
        var cg = chunk.Pos;
        for (var i = 0; i < 3; ++i)
        {
            cg[i] %= 3;
            if (cg[i] < 0) cg[i] += 3;
        }

        _groupedChunks[cg.X * 9 + cg.Y * 3 + cg.Z].Add(chunk);
        foreach (var a in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
            if (a != new Vector3i(1))
            {
                var oPos = chunk.Pos + a - new Vector3i(1);
                var oa = new Vector3i(2) - a;
                if (_chunks.TryGetValue(oPos, out var oChunk))
                {
                    chunk.Neighbourhood.Chunks[a.X, a.Y, a.Z] = oChunk;
                    oChunk.Neighbourhood.Chunks[oa.X, oa.Y, oa.Z] = chunk;
                }
            }
    }

    private void OnChunkRemoved0(Chunk chunk)
    {
        chunk.OnRemovedFromWorld();
        chunk.ExistsInWorld = false;
        foreach (var a in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
            if (a != new Vector3i(1))
            {
                var oPos = chunk.Pos + a - new Vector3i(1);
                var oa = new Vector3i(2) - a;
                if (_chunks.TryGetValue(oPos, out var oChunk)) oChunk.Neighbourhood.Chunks[oa.X, oa.Y, oa.Z] = null!;
            }
    }

    public void AddEntity(Entity entity, bool loaded = false)
    {
        GetOrLoadGeneratedChunk(entity.Pos.ChunkPos).AddEntity(entity, loaded);
    }

    public void RemoveEntity(Entity entity)
    {
        GetOrLoadGeneratedChunk(entity.Pos.ChunkPos).RemoveEntity(entity);
    }
}