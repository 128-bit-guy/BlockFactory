using BlockFactory.CubeMath;
using OpenTK.Mathematics;
using BlockFactory.Util.Math_;
using Algorithms = BlockFactory.Util.Algorithms;

namespace BlockFactory.Entity_.Player
{
    public static class PlayerChunkLoading
    {
        public const int MaxChunkLoadDistance = 16;
        public static readonly List<Vector3i> ChunkOffsets = new();
        public static readonly List<int> MaxPoses = new();
        private static readonly int[] DistanceSqrs = new int[MaxChunkLoadDistance + 1];

        private static int GetDistanceSortKey(Vector3i pos) {
            int min = (int)1e9;
            for (int i = -1; i <= 1; ++i) {
                for (int j = -1; j <= 1; ++j) {
                    for (int k = -1; k <= 1; ++k) {
                        Vector3i oPos = (i, j, k);
                        Vector3i oPos2 = oPos + pos;
                        min = Math.Min(min, oPos2.SquareLength());
                    }
                }
            }

            int res = 0;
            while (res <= MaxChunkLoadDistance && min > DistanceSqrs[res])
            {
                ++res;
            }

            return res;
        }

        private static int GetRemSortKey(Vector3i a)
        {
            for (int i = 0; i < 3; ++i)
            {
                a[i] %= 3;
                if (a[i] < 0)
                {
                    a[i] += 3;
                }
            }

            return a.X + 3 * a.Y + 9 * a.Z;
        }

        private static int Compare(Vector3i x, Vector3i y) {
            int a = GetDistanceSortKey(x) - GetDistanceSortKey(y);
            if (a == 0)
            {
                return GetRemSortKey(x) - GetRemSortKey(y);
            }

            return a;
        }

        public static void Init()
        {
            for (int dist = 0; dist <= MaxChunkLoadDistance; ++dist)
            {
                DistanceSqrs[dist] = dist * dist;
            }
            for (int i = -MaxChunkLoadDistance - 1; i <= MaxChunkLoadDistance + 1; ++i) {
                for (int j = -MaxChunkLoadDistance - 1; j <= MaxChunkLoadDistance + 1; ++j) {
                    for (int k = -MaxChunkLoadDistance - 1; k <= MaxChunkLoadDistance + 1; ++k) {
                        Vector3i pos = (i, j, k);
                        if (GetDistanceSortKey(pos) <= MaxChunkLoadDistance) {
                            ChunkOffsets.Add(pos);
                        }
                    }
                }
            }

            Random random = new Random(228);
            Algorithms.Shuffle(ChunkOffsets, random);
            ChunkOffsets.Sort(Compare);
            int offsetsPos = 0;
            for (int i = 0; i <= MaxChunkLoadDistance; ++i) {
                while (offsetsPos < ChunkOffsets.Count && GetDistanceSortKey(ChunkOffsets[offsetsPos]) <= i) {
                    ++offsetsPos;
                }
                MaxPoses.Add(offsetsPos);
            }
        }
    }
}
