using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Util;
using BlockFactory.Util.Dependency;
using BlockFactory.World_.Api;
using BlockFactory.World_.Chunk_;
using BlockFactory.World_.Gen;
using BlockFactory.World_.Save;
using OpenTK.Mathematics;

namespace BlockFactory.World_;

public class World : IBlockStorage, IDisposable
{
    public delegate void ChunkEventHandler(Chunk chunk);

    private readonly Dictionary<Vector3i, Chunk> _chunks = new();
    private readonly Dictionary<long, PlayerEntity> _players = new();
    public readonly GameInstance GameInstance;
    public readonly WorldGenerator Generator;
    public readonly WorldSaveManager SaveManager;
    private long _lastId;

    public World(GameInstance gameInstance, int seed, string saveName)
    {
        GameInstance = gameInstance;
        Generator = new WorldGenerator(seed);
        OnChunkAdded += OnChunkAdded0;
        OnChunkRemoved += OnChunkRemoved0;
        SaveManager = new WorldSaveManager(this, saveName);
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
        SaveManager.Dispose();
    }

    public event ChunkEventHandler OnChunkAdded = _ => { };
    public event ChunkEventHandler OnChunkRemoved = _ => { };

    public void AddPlayer(PlayerEntity player)
    {
        player.Id = _lastId++;
        player.GameInstance = GameInstance;
        player.World = this;
        _players[player.Id] = player;
    }

    public void RemovePlayer(PlayerEntity player)
    {
        player.OnRemoveFromWorld();
        _players.Remove(player.Id);
    }

    public Chunk GetOrLoadChunk(Vector3i pos)
    {
        if (Thread.CurrentThread != GameInstance.MainThread)
            throw new InvalidOperationException("Can not get chunk from not main thread!");
        if (_chunks.TryGetValue(pos, out var ch)) return ch;
        var regionPos = pos.BitShiftRight(ChunkRegion.SizeLog2);
        var region = SaveManager.GetRegion(regionPos);
        region.EnsureLoaded();
        ((IDependable)region).OnDependencyAdded();
        var posInRegion = pos.BitAnd(ChunkRegion.Mask);
        var data = region.GetOrCreateChunkData(posInRegion, out var created);
        var chunk = _chunks[pos] = new Chunk(data, pos, this);
        OnChunkAdded(chunk);
        if (created)
        {
            chunk.RunGenerationTask();
        }

        return chunk;
    }

    public Chunk GetOrLoadGeneratedChunk(Vector3i pos)
    {
        var ch = GetOrLoadChunk(pos);
        ch.EnsureGenerated();
        return ch;
    }

    public void Tick()
    {
        foreach (var (id, player) in _players) player.Tick();

        if (GameInstance.Kind.DoesProcessLogic())
        {
            foreach (var (id, player) in _players) player.LoadChunks();

            foreach (var (id, player) in _players)
            {
                player.ProcessScheduledAddedVisibleChunks();
                player.UnloadChunks(false);
            }

            UnloadChunks(false);
            SaveManager.UnloadRegions();
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
            var regionPos = pos.BitShiftRight(ChunkRegion.SizeLog2);
            var region = SaveManager.GetRegion(regionPos);
            ((IDependable)region).OnDependencyRemoved();
            _chunks.Remove(pos);
        }
    }

    public void AddChunk(Chunk chunk)
    {
        _chunks.Add(chunk.Pos, chunk);
        OnChunkAdded(chunk);
    }

    public void RemoveChunk(Vector3i chunkPos)
    {
        var ch = _chunks[chunkPos];
        OnChunkRemoved(ch);
        _chunks.Remove(chunkPos);
    }

    private void OnChunkAdded0(Chunk chunk)
    {
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
        foreach (var a in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
            if (a != new Vector3i(1))
            {
                var oPos = chunk.Pos + a - new Vector3i(1);
                var oa = new Vector3i(2) - a;
                if (_chunks.TryGetValue(oPos, out var oChunk)) oChunk.Neighbourhood.Chunks[oa.X, oa.Y, oa.Z] = null!;
            }
    }
}