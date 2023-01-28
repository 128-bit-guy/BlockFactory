using BlockFactory.CubeMath;
using OpenTK.Mathematics;
using BlockFactory.Util.Math_;

namespace BlockFactory.Util
{
    public static class WorldEnumerators
    {
        public static IEnumerable<Vector3i> InclusiveEnumerable(this Box3i box)
        {
            for (int i = box.Min.X; i <= box.Max.X; ++i) {
                for (int j = box.Min.Y; j <= box.Max.Y; ++j) {
                    for (int k = box.Min.Z; k <= box.Max.Z; ++k) { 
                        yield return new Vector3i(i, j, k);
                    }
                }
            }
        }

        public static IEnumerable<Vector3i> GetSphereEnumerator(Vector3i center, int radius) {
            Box3i box = new Box3i(center - new Vector3i(radius), center + new Vector3i(radius));
            foreach (Vector3i pos in box.InclusiveEnumerable()) {
                if ((pos - center).SquareLength() <= radius * radius) {
                    yield return pos;
                }
            }
        }
    }
}
