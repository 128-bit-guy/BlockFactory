﻿using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class PlayerChunkLoader : IDisposable
{
    public readonly PlayerEntity Player;
    private readonly Chunk?[] _watchedChunks = new Chunk?[1 << (3 * PlayerChunkLoading.CkdPowerOf2)];
    private readonly List<Chunk> _loadingChunks = new();
    private readonly List<Chunk> _tempLoadingChunks = new();
    private readonly bool[] _chunkIsVisible = new bool[1 << (3 * PlayerChunkLoading.CkdPowerOf2)];
    private Vector3D<int> _lastChunkPos;
    public int Progress { get; private set; }
    public int MaxLoadingChunks = 2;

    public PlayerChunkLoader(PlayerEntity player)
    {
        Player = player;
    }

    public void Update()
    {
        var blockPos =
            new Vector3D<double>(Math.Floor(Player.Pos.X), Math.Floor(Player.Pos.Y), Math.Floor(Player.Pos.Z))
                .As<int>();
        var chunkPos = blockPos.ShiftRight(Constants.ChunkSizeLog2);
        if (chunkPos != _lastChunkPos)
        {
            var delta = chunkPos - _lastChunkPos;

            _lastChunkPos = chunkPos;
            var big = false;
            for (int i = 0; i < 3; ++i)
            {
                if (Math.Abs(delta[i]) > 1)
                {
                    big = true;
                    break;
                }
            }

            if (big)
            {
                Reset();
            }
            else
            {
                Move(delta);
            }
        }
        MakeChunksLoaded();
        int leftProgressDelta = 8;
        while (Progress < PlayerChunkLoading.ChunkDeltas.Length)
        {
            var delta = PlayerChunkLoading.ChunkDeltas[Progress];
            var c = Player.World!.GetChunk(chunkPos + delta)!;
            if (!IsChunkWatched(c))
            {
                if (_loadingChunks.Count >= MaxLoadingChunks)
                {
                    break;
                }

                WatchChunk(c);
                _loadingChunks.Add(c);
            }

            ++Progress;
            --leftProgressDelta;
            if (leftProgressDelta == 0)
            {
                break;
            }
        }
    }

    private void MakeChunksLoaded()
    {
        _tempLoadingChunks.AddRange(_loadingChunks);
        foreach (var c in _tempLoadingChunks)
        {
            MakeChunkVisible(c);
            _loadingChunks.Remove(c);
        }
        _tempLoadingChunks.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & PlayerChunkLoading.CkdMask) |
               (((pos.Y & PlayerChunkLoading.CkdMask) |
                 ((pos.Z & PlayerChunkLoading.CkdMask) << PlayerChunkLoading.CkdPowerOf2)) <<
                PlayerChunkLoading.CkdPowerOf2);
    }

    private void UnWatchChunk(Chunk c)
    {
        if (IsChunkVisible(c))
        {
            MakeChunkNotVisible(c);
        }
        else
        {
            _loadingChunks.Remove(c);
        }
        c.RemoveWatchingPlayer(Player);
        _watchedChunks[GetArrIndex(c.Position)] = null;
    }

    private void WatchChunk(Chunk c)
    {
        _watchedChunks[GetArrIndex(c.Position)] = c;
        c.AddWatchingPlayer(Player);
    }

    private void MakeChunkVisible(Chunk c)
    {
        _chunkIsVisible[GetArrIndex(c.Position)] = true;
    }

    private void MakeChunkNotVisible(Chunk c)
    {
        _chunkIsVisible[GetArrIndex(c.Position)] = false;
    }

    public bool IsChunkWatched(Chunk c)
    {
        return _watchedChunks[GetArrIndex(c.Position)] == c;
    }

    public bool IsChunkVisible(Chunk c)
    {
        return IsChunkWatched(c) && _chunkIsVisible[GetArrIndex(c.Position)];
    }

    private void Reset()
    {
        foreach (var chunk in _watchedChunks)
        {
            if (chunk != null)
            {
                UnWatchChunk(chunk);
            }
        }
        Progress = 0;
    }

    public void Dispose()
    {
        Reset();
    }

    private void Move(Vector3D<int> delta)
    {
        foreach (var remDelta in PlayerChunkLoading.ChunkToRemoveDeltas
                     [delta.X + 1, delta.Y + 1, delta.Z + 1])
        {
            var remAbs = remDelta + _lastChunkPos;
            var c = _watchedChunks[GetArrIndex(remAbs)];
            if(c == null) continue;
            UnWatchChunk(c);
        }

        Progress = PlayerChunkLoading.ProgressChanges[delta.X + 1, delta.Y + 1, delta.Z + 1, Progress];
    }
}