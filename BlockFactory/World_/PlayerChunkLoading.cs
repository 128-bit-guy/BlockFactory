using Silk.NET.Maths;

namespace BlockFactory.World_;

public class PlayerChunkLoading
{
    public const int VisibleChunkRadius = 12;
    public const int ExtensionChunkRadius = 1;
    public const int LoadedChunkRadius = VisibleChunkRadius + ExtensionChunkRadius;
    public const int ChunkKeepingRadius = LoadedChunkRadius + 3;
    public const int ChunkKeepingDiameter = 2 * ChunkKeepingRadius + 1;
    public static readonly Vector3D<int>[] ChunkDeltas;

    private static int GetSortKey(Vector3D<int> delta)
    {
        var min = delta.LengthSquared;
        for (var i = -ExtensionChunkRadius; i <= ExtensionChunkRadius; ++i)
        for (var j = -ExtensionChunkRadius; j <= ExtensionChunkRadius; ++j)
        for (var k = -ExtensionChunkRadius; k <= ExtensionChunkRadius; ++k)
        {
            var delta2 = delta + new Vector3D<int>(i, j, k);
            min = Math.Min(min, delta2.LengthSquared);
        }

        return min;
    }

    private static int CompareVectors(Vector3D<int> a, Vector3D<int> b)
    {
        return GetSortKey(a) - GetSortKey(b);
    }

    static PlayerChunkLoading()
    {
        var chunkDeltasList = new List<Vector3D<int>>();
        for (var i = -LoadedChunkRadius; i <= LoadedChunkRadius; ++i)
        for (var j = -LoadedChunkRadius; j <= LoadedChunkRadius; ++j)
        for (var k = -LoadedChunkRadius; k <= LoadedChunkRadius; ++k)
        {
            var delta = new Vector3D<int>(i, j, k);
            if (GetSortKey(delta) <= VisibleChunkRadius * VisibleChunkRadius)
            {
                chunkDeltasList.Add(delta);
            }
        }
        chunkDeltasList.Sort(CompareVectors);
        ChunkDeltas = chunkDeltasList.ToArray();
    }
}