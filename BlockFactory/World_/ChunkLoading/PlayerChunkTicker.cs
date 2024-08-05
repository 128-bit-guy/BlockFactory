using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Content.Entity_;
using BlockFactory.Utils;
using Silk.NET.Maths;

namespace BlockFactory.World_.ChunkLoading;

public class PlayerChunkTicker : IDisposable
{
    private readonly Chunk?[] _watchedChunks = new Chunk?[1 << (3 * PlayerChunkTicking.CkdPowerOf2)];
    public readonly PlayerEntity Player;
    private Vector3D<int> _lastChunkPos;

    public PlayerChunkTicker(PlayerEntity player)
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

        var leftProgressDelta = 64;
        while (Progress < PlayerChunkTicking.ChunkDeltas.Length)
        {
            var delta = PlayerChunkTicking.ChunkDeltas[Progress];
            var c = Player.World!.GetChunk(chunkPos + delta, false);
            if (c == null) break;

            if (!IsChunkWatched(c)) WatchChunk(c);

            ++Progress;
            --leftProgressDelta;
            if (leftProgressDelta == 0) break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & PlayerChunkTicking.CkdMask) |
               (((pos.Y & PlayerChunkTicking.CkdMask) |
                 ((pos.Z & PlayerChunkTicking.CkdMask) << PlayerChunkTicking.CkdPowerOf2)) <<
                PlayerChunkTicking.CkdPowerOf2);
    }

    private void UnWatchChunk(Chunk c)
    {
        c.ChunkStatusInfo.RemoveTickingDependency();
        _watchedChunks[GetArrIndex(c.Position)] = null;
    }

    public bool IsChunkWatched(Chunk c)
    {
        return _watchedChunks[GetArrIndex(c.Position)] == c;
    }

    private void WatchChunk(Chunk c)
    {
        _watchedChunks[GetArrIndex(c.Position)] = c;
        c.ChunkStatusInfo.AddTickingDependency();
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
        foreach (var remDelta in PlayerChunkTicking.ChunkToRemoveDeltas
                     [delta.X + 1, delta.Y + 1, delta.Z + 1])
        {
            var remAbs = remDelta + _lastChunkPos;
            var c = _watchedChunks[GetArrIndex(remAbs)];
            if (c == null) continue;
            UnWatchChunk(c);
        }

        Progress = PlayerChunkTicking.ProgressChanges[delta.X + 1, delta.Y + 1, delta.Z + 1, Progress];
    }
}