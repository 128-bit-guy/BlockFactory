using BlockFactory.CubeMath;
using OpenTK.Mathematics;
using BlockFactory.Util.Math_;

namespace BlockFactory.Entity_.Player
{
    public static class PlayerChunkLoading
    {
        public const int MaxChunkLoadDistance = 16;
        public static readonly List<Vector3i> ChunkOffsets = new();
        public static readonly List<int> MaxPoses = new();

        private static int GetSortKey(Vector3i pos) {
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
            return min;
        }

        private static int Compare(Vector3i x, Vector3i y) {
            return GetSortKey(x) - GetSortKey(y);
        }

        public static void Init() {
            for (int i = -MaxChunkLoadDistance - 1; i <= MaxChunkLoadDistance + 1; ++i) {
                for (int j = -MaxChunkLoadDistance - 1; j <= MaxChunkLoadDistance + 1; ++j) {
                    for (int k = -MaxChunkLoadDistance - 1; k <= MaxChunkLoadDistance + 1; ++k) {
                        Vector3i pos = (i, j, k);
                        if (GetSortKey(pos) <= MaxChunkLoadDistance * MaxChunkLoadDistance) {
                            ChunkOffsets.Add(pos);
                        }
                    }
                }
            }
            ChunkOffsets.Sort(Compare);
            int offsetsPos = 0;
            for (int i = 0; i <= MaxChunkLoadDistance; ++i) {
                while (offsetsPos < ChunkOffsets.Count && GetSortKey(ChunkOffsets[offsetsPos]) <= i * i) {
                    ++offsetsPos;
                }
                MaxPoses.Add(offsetsPos);
            }
        }
    }
}
