using BlockFactory.CubeMath;
using OpenTK.Mathematics;

namespace BlockFactory.Util;

public static class WorldEnumerators
{
    public static IEnumerable<Vector3i> InclusiveEnumerable(this Box3i box)
    {
        for (var i = box.Min.X; i <= box.Max.X; ++i)
        for (var j = box.Min.Y; j <= box.Max.Y; ++j)
        for (var k = box.Min.Z; k <= box.Max.Z; ++k)
            yield return new Vector3i(i, j, k);
    }

    public static IEnumerable<Vector3i> GetSphereEnumerator(Vector3i center, int radius)
    {
        var box = new Box3i(center - new Vector3i(radius), center + new Vector3i(radius));
        foreach (var pos in box.InclusiveEnumerable())
            if ((pos - center).SquareLength() <= radius * radius)
                yield return pos;
    }
}