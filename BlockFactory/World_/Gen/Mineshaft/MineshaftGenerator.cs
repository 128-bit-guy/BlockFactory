using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Utils;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen.Mineshaft;

public class MineshaftGenerator : WorldGenElement
{
    private const int SearchRadius = 5;
    public MineshaftGenerator(IWorldGenerator generator, long uniqueNumber) : base(generator, uniqueNumber)
    {
    }

    private void AddPiecesForOrigPos(Chunk c, Vector3D<int> origPos, List<IMineshaftElement> elements)
    {
        if(origPos.Y > -3) return;
        var rng = GetChunkRandom(origPos);
        if(rng.Next(1000) != 0) return;
        var q = new Queue<IMineshaftElement>();
        var beginPos = origPos.ShiftLeft(Constants.ChunkSizeLog2);
        beginPos.X += rng.Next(Constants.ChunkSize);
        beginPos.Y += rng.Next(Constants.ChunkSize);
        beginPos.Z += rng.Next(Constants.ChunkSize);
        q.Enqueue(new MineshaftRoom(beginPos, CubeFace.Top));
        var pieceCnt = 0;
        while (q.Count > 0)
        {
            var element = q.Dequeue();
            var delta = element.GetCenter() - beginPos;
            var big = false;
            for (var i = 0; i < 3; ++i)
            {
                if (Math.Abs(delta[i]) > Constants.ChunkSize * (SearchRadius - 1))
                {
                    big = true;
                    break;
                }
            }

            if (big)
            {
                continue;
            }
            if (c.IsBlockLoaded(element.GetCenter()))
            {
                elements.Add(element);
            }

            if (pieceCnt < 100)
            {
                element.AddNeighbours(rng, q);
            }
            ++pieceCnt;
        }
    }

    public void Generate(Chunk c, Random rng)
    {
        var elements = new List<IMineshaftElement>();
        for (var i = -SearchRadius; i <= SearchRadius; ++i)
        for (var j = -SearchRadius; j <= SearchRadius; ++j)
        for (var k = -SearchRadius; k <= SearchRadius; ++k)
        {
            var origChunkPos = c.Position + new Vector3D<int>(i, j, k);
            AddPiecesForOrigPos(c, origChunkPos, elements);
        }

        foreach (var element in elements)
        {
            element.Generate(c, rng);
        }
    }
}