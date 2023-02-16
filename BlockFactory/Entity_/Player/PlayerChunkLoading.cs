using BlockFactory.CubeMath;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Entity_.Player;

public static class PlayerChunkLoading
{
    public const int MaxChunkLoadDistance = 16;
    public static readonly List<Vector3i> ChunkOffsets = new();
    public static readonly List<int> MaxPoses = new();
    private static readonly int[] DistanceSqrs = new int[MaxChunkLoadDistance + 1];

    private static int GetDistanceSortKey(Vector3i pos)
    {
        var min = (int)1e9;
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            Vector3i oPos = (i, j, k);
            var oPos2 = oPos + pos;
            min = Math.Min(min, oPos2.SquareLength());
        }

        var res = 0;
        while (res <= MaxChunkLoadDistance && min > DistanceSqrs[res]) ++res;

        return res;
    }

    private static int GetRemSortKey(Vector3i a)
    {
        for (var i = 0; i < 3; ++i)
        {
            a[i] %= 3;
            if (a[i] < 0) a[i] += 3;
        }

        return a.X + 3 * a.Y + 9 * a.Z;
    }

    private static int Compare(Vector3i x, Vector3i y)
    {
        var a = GetDistanceSortKey(x) - GetDistanceSortKey(y);
        if (a == 0) return GetRemSortKey(x) - GetRemSortKey(y);

        return a;
    }

    public static void Init()
    {
        for (var dist = 0; dist <= MaxChunkLoadDistance; ++dist) DistanceSqrs[dist] = dist * dist;
        for (var i = -MaxChunkLoadDistance - 1; i <= MaxChunkLoadDistance + 1; ++i)
        for (var j = -MaxChunkLoadDistance - 1; j <= MaxChunkLoadDistance + 1; ++j)
        for (var k = -MaxChunkLoadDistance - 1; k <= MaxChunkLoadDistance + 1; ++k)
        {
            Vector3i pos = (i, j, k);
            if (GetDistanceSortKey(pos) <= MaxChunkLoadDistance) ChunkOffsets.Add(pos);
        }

        var random = new Random(228);
        ChunkOffsets.Shuffle(random);
        ChunkOffsets.Sort(Compare);
        var offsetsPos = 0;
        for (var i = 0; i <= MaxChunkLoadDistance; ++i)
        {
            while (offsetsPos < ChunkOffsets.Count && GetDistanceSortKey(ChunkOffsets[offsetsPos]) <= i) ++offsetsPos;
            MaxPoses.Add(offsetsPos);
        }
    }
}