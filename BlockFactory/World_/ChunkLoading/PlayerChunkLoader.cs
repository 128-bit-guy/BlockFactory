using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.World_.ChunkLoading;

public class PlayerChunkLoader : IDisposable
{
    private readonly bool[] _chunkIsVisible = new bool[1 << (3 * PlayerChunkLoading.CkdPowerOf2)];
    private readonly List<Chunk> _loadingChunks = new();
    private readonly List<Chunk> _notReadyLoadedChunks = new();
    private readonly List<Chunk> _tempLoadingChunks = new();
    private readonly Chunk?[] _watchedChunks = new Chunk?[1 << (3 * PlayerChunkLoading.CkdPowerOf2)];
    public readonly PlayerEntity Player;
    private Vector3D<int> _lastChunkPos;
    #if DEBUG
    public int MaxLoadingChunks = 8;
    #else
    public int MaxLoadingChunks = 16;
    #endif

    public PlayerChunkLoader(PlayerEntity player)
    {
        Player = player;
    }

    public int Progress { get; private set; }

    public void Dispose()
    {
        Reset();
    }

    public void Update()
    {
        if(Player.World!.LogicProcessor.LogicalSide == LogicalSide.Client) return;
        var blockPos =
            new Vector3D<double>(Math.Floor(Player.Pos.X), Math.Floor(Player.Pos.Y), Math.Floor(Player.Pos.Z))
                .As<int>();
        var chunkPos = blockPos.ShiftRight(Constants.ChunkSizeLog2);
        if (chunkPos != _lastChunkPos)
        {
            var delta = chunkPos - _lastChunkPos;

            _lastChunkPos = chunkPos;
            var big = false;
            for (var i = 0; i < 3; ++i)
                if (Math.Abs(delta[i]) > 1)
                {
                    big = true;
                    break;
                }

            if (big)
                Reset();
            else
                Move(delta);
        }

        MakeChunksLoaded();
        var leftProgressDelta = 64;
        while (Progress < PlayerChunkLoading.ChunkDeltas.Length)
        {
            var delta = PlayerChunkLoading.ChunkDeltas[Progress];
            var c = Player.World!.GetChunk(chunkPos + delta)!;
            if (!IsChunkWatched(c))
            {
                if (_loadingChunks.Count >= MaxLoadingChunks) break;

                WatchChunk(c);
                _loadingChunks.Add(c);
            }

            ++Progress;
            --leftProgressDelta;
            if (leftProgressDelta == 0) break;
        }
    }

    private void MakeChunksLoaded()
    {
        _tempLoadingChunks.AddRange(_loadingChunks);
        foreach (var c in _tempLoadingChunks)
        {
            if(c.Data == null) continue;
            _notReadyLoadedChunks.Add(c);
            _loadingChunks.Remove(c);
        }

        _tempLoadingChunks.Clear();
        _tempLoadingChunks.AddRange(_notReadyLoadedChunks);
        foreach (var c in _tempLoadingChunks)
        {
            if(!c.Data!.FullyDecorated) continue;
            MakeChunkVisible(c);
            _notReadyLoadedChunks.Remove(c);
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
            MakeChunkNotVisible(c);
        else
        {
            _loadingChunks.Remove(c);
            _notReadyLoadedChunks.Remove(c);
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
        Player.OnChunkBecameVisible(c);
    }

    private void MakeChunkNotVisible(Chunk c)
    {
        Player.OnChunkBecameInvisible(c);
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
            if (chunk != null)
                UnWatchChunk(chunk);
        Progress = 0;
    }

    private void Move(Vector3D<int> delta)
    {
        foreach (var remDelta in PlayerChunkLoading.ChunkToRemoveDeltas
                     [delta.X + 1, delta.Y + 1, delta.Z + 1])
        {
            var remAbs = remDelta + _lastChunkPos;
            var c = _watchedChunks[GetArrIndex(remAbs)];
            if (c == null) continue;
            UnWatchChunk(c);
        }

        Progress = PlayerChunkLoading.ProgressChanges[delta.X + 1, delta.Y + 1, delta.Z + 1, Progress];
    }

    [ExclusiveTo(Side.Client)]
    public void AddVisibleChunk(Chunk c)
    {
        WatchChunk(c);
        MakeChunkVisible(c);
    }

    [ExclusiveTo(Side.Client)]
    public void RemoveVisibleChunk(Chunk c)
    {
        UnWatchChunk(c);
    }
}