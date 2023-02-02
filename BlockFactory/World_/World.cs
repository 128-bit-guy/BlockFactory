﻿using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Util;
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
    private readonly Stack<ChunkGenerationLevel> _generationLevelStack = new();
    private readonly Dictionary<long, PlayerEntity> _players = new();
    public readonly GameInstance GameInstance;
    public readonly WorldGenerator Generator;
    public readonly WorldSaveManager SaveManager;
    private long _lastId;

    public World(GameInstance gameInstance, int seed, string saveName)
    {
        GameInstance = gameInstance;
        Generator = new WorldGenerator(seed);
        _generationLevelStack.Push(ChunkGenerationLevel.Decorated);
        OnChunkReadyForUse += OnChunkReadyForUse0;
        OnChunkNotReadyForUse += OnChunkNotReadyForUse0;
        SaveManager = new WorldSaveManager(this, saveName);
    }

    public BlockState GetBlockState(Vector3i pos)
    {
        return GetOrLoadChunk(pos.BitShiftRight(Chunk.SizeLog2)).GetBlockState(pos);
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        GetOrLoadChunk(pos.BitShiftRight(Chunk.SizeLog2)).SetBlockState(pos, state);
    }

    public void Dispose()
    {
        SaveManager.Dispose();
    }

    public event ChunkEventHandler OnChunkReadyForUse = _ => { };
    public event ChunkEventHandler OnChunkNotReadyForUse = _ => { };

    public ChunkGenerationLevel GetGenerationLevel()
    {
        return _generationLevelStack.Peek();
    }

    public void PushGenerationLevel(ChunkGenerationLevel level)
    {
        _generationLevelStack.Push(level);
    }

    public void PopGenerationLevel()
    {
        _generationLevelStack.Pop();
    }

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

    public Chunk GetOrLoadAnyLevelChunk(Vector3i pos)
    {
        if (_chunks.TryGetValue(pos, out var ch)) return ch;

        var chunk = _chunks[pos] = new Chunk(new ChunkData(), pos, this);
        OnChunkReadyForUse(chunk);
        return chunk;
    }

    public Chunk GetOrLoadChunk(Vector3i pos, bool process = true)
    {
        if (Thread.CurrentThread != GameInstance.MainThread)
            throw new InvalidOperationException("Can not get chunk from not main thread!");
        var ch = GetOrLoadAnyLevelChunk(pos);
        ch.EnsureMinLevel(GetGenerationLevel(), process);
        return ch;
    }

    public void Tick()
    {
        foreach (var (id, player) in _players) player.Tick();

        if (GameInstance.Kind.DoesProcessLogic())
        {
            foreach (var (id, player) in _players) player.LoadChunks();

            Generator.ProcessScheduled();

            foreach (var (id, player) in _players)
            {
                player.ProcessScheduledAddedVisibleChunks();
                player.UnloadChunks(false);
            }
        }
    }

    public void AddChunk(Chunk chunk)
    {
        _chunks.Add(chunk.Pos, chunk);
        OnChunkReadyForUse(chunk);
    }

    public void RemoveChunk(Vector3i chunkPos)
    {
        var ch = _chunks[chunkPos];
        OnChunkNotReadyForUse(ch);
        _chunks.Remove(chunkPos);
    }

    private void OnChunkReadyForUse0(Chunk chunk)
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

    private void OnChunkNotReadyForUse0(Chunk chunk)
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