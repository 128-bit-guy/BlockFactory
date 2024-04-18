using BlockFactory.World_.ChunkLoading;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render;

public static class WorldRendering
{
    public static readonly Vector3D<int>[] ChunkDeltas;
    private const int LoadedChunkRadius = PlayerChunkLoading.LoadedChunkRadius + 3;

    static WorldRendering()
    {
        var chunkDeltasList = new List<Vector3D<int>>();
        for (var i = -LoadedChunkRadius; i <= LoadedChunkRadius; ++i)
        for (var j = -LoadedChunkRadius; j <= LoadedChunkRadius; ++j)
        for (var k = -LoadedChunkRadius; k <= LoadedChunkRadius; ++k)
        {
            var delta = new Vector3D<int>(i, j, k);
            if (GetSortKey(delta) <= LoadedChunkRadius * LoadedChunkRadius) chunkDeltasList.Add(delta);
        }
        
        chunkDeltasList.Sort(CompareVectors);
        ChunkDeltas = chunkDeltasList.ToArray();
    }

    private static int GetSortKey(Vector3D<int> pos)
    {
        return pos.LengthSquared;
    }

    private static int CompareVectors(Vector3D<int> a, Vector3D<int> b)
    {
        return GetSortKey(a) - GetSortKey(b);
    }
}