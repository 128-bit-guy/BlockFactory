using BlockFactory.Entity_;
using Silk.NET.Maths;

namespace BlockFactory.World_.ChunkLoading;

public class PlayerChunkTicking
{
    public const int VisibleChunkRadius = 4;
    public const int LoadedChunkRadius = VisibleChunkRadius;
    public const int ChunkKeepingRadius = LoadedChunkRadius;
    public const int ChunkKeepingDiameter = 2 * ChunkKeepingRadius + 1;
    public static readonly int CkdPowerOf2 = 30;
    public static readonly int CkdMask;
    public static readonly Vector3D<int>[] ChunkDeltas;
    public static readonly Vector3D<int>[,,][] ChunkToRemoveDeltas;
    public static readonly int[,,,] ProgressChanges;

    static PlayerChunkTicking()
    {
        #region ChunkKeepingDiameter power of 2

        for (var i = 4; i >= 0; --i)
        {
            var nCkdPowerOf2 = CkdPowerOf2 - (1 << i);
            if (1 << nCkdPowerOf2 >= ChunkKeepingDiameter + 1) CkdPowerOf2 = nCkdPowerOf2;
        }

        CkdMask = (1 << CkdPowerOf2) - 1;

        #endregion

        #region ChunkDeltas

        var chunkDeltasList = new List<Vector3D<int>>();
        for (var i = -LoadedChunkRadius; i <= LoadedChunkRadius; ++i)
        for (var j = -LoadedChunkRadius; j <= LoadedChunkRadius; ++j)
        for (var k = -LoadedChunkRadius; k <= LoadedChunkRadius; ++k)
        {
            var delta = new Vector3D<int>(i, j, k);
            if (GetSortKey(delta) <= VisibleChunkRadius * VisibleChunkRadius) chunkDeltasList.Add(delta);
        }

        chunkDeltasList.Sort(CompareVectors);
        ChunkDeltas = chunkDeltasList.ToArray();

        #endregion

        #region ChunkProgresses

        var chunkProgresses = new Dictionary<Vector3D<int>, int>();
        for (var i = 0; i < ChunkDeltas.Length; ++i) chunkProgresses[ChunkDeltas[i]] = i;

        #endregion

        #region ChunkToRemoveDeltas

        ChunkToRemoveDeltas = new Vector3D<int>[3, 3, 3][];

        for (var dx = -1; dx <= 1; ++dx)
        for (var dy = -1; dy <= 1; ++dy)
        for (var dz = -1; dz <= 1; ++dz)
        {
            var d = new Vector3D<int>(dx, dy, dz);
            var l = new List<Vector3D<int>>();
            foreach (var delta in ChunkDeltas)
                if (!chunkProgresses.ContainsKey(delta + d))
                    l.Add(delta + d);

            ChunkToRemoveDeltas[-dx + 1, -dy + 1, -dz + 1] = l.ToArray();
        }

        #endregion

        #region ProgressChanges

        ProgressChanges = new int[3, 3, 3, ChunkDeltas.Length + 1];

        for (var dx = -1; dx <= 1; ++dx)
        for (var dy = -1; dy <= 1; ++dy)
        for (var dz = -1; dz <= 1; ++dz)
        {
            var d = new Vector3D<int>(dx, dy, dz);
            var nProgress = 0;
            for (var oldProgress = 0; oldProgress <= ChunkDeltas.Length; ++oldProgress)
            {
                while (nProgress < ChunkDeltas.Length && chunkProgresses.ContainsKey(ChunkDeltas[nProgress] - d) &&
                       chunkProgresses[ChunkDeltas[nProgress] - d] < oldProgress)
                    ++nProgress;

                ProgressChanges[-dx + 1, -dy + 1, -dz + 1, oldProgress] = nProgress;
            }
        }

        #endregion
    }

    #region Sorting

    private static int GetSortKey(Vector3D<int> delta)
    {
        var min = delta.LengthSquared;

        return min;
    }

    private static int CompareVectors(Vector3D<int> a, Vector3D<int> b)
    {
        return GetSortKey(a) - GetSortKey(b);
    }

    #endregion
}