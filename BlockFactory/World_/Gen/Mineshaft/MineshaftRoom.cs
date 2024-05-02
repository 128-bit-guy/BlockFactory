using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen.Mineshaft;

public class MineshaftRoom : IMineshaftElement
{
    private readonly Vector3D<int> _center;
    private readonly CubeFace _fromDir;

    public MineshaftRoom(Vector3D<int> center, CubeFace fromDir)
    {
        _center = center;
        _fromDir = fromDir;
    }

    public Vector3D<int> GetCenter()
    {
        return _center;
    }

    public void AddNeighbours(Random rng, Queue<IMineshaftElement> elements)
    {
        foreach (var face in CubeFaceUtils.Values())
        {
            if(face.GetAxis() == 1) continue;
            if(face == _fromDir) continue;
            if(rng.Next(3) == 0) continue;
            var nBeginning = _center + face.GetDelta() * 4;
            elements.Enqueue(new MineshaftCorridor(nBeginning, face));
        }
    }

    public void Generate(Chunk c, Random rng)
    {
        for (var i = -3; i <= 3; ++i)
        {
            for (var k = -3; k <= 3; ++k)
            {
                for (var j = -1; j <= 1; ++j)
                {
                    var absPos = _center + new Vector3D<int>(i, j, k);
                    c.Neighbourhood.SetBlock(absPos, 0);
                }

                {
                    var absPos = _center + new Vector3D<int>(i, -2, k);
                    if (c.Neighbourhood.GetBlock(absPos) == 0 || c.Neighbourhood.GetBlock(absPos) == Blocks.Water.Id)
                    {
                        c.Neighbourhood.SetBlock(absPos, Blocks.Planks);
                    }
                }
            }
        }
        if (rng.Next(2) == 0)
        {
            c.Neighbourhood.SetBlock(_center + new Vector3D<int>(3, -1, 3), Blocks.Torch);
        }
        if (rng.Next(2) == 0)
        {
            c.Neighbourhood.SetBlock(_center + new Vector3D<int>(-3, -1, 3), Blocks.Torch);
        }
        if (rng.Next(2) == 0)
        {
            c.Neighbourhood.SetBlock(_center + new Vector3D<int>(-3, -1, -3), Blocks.Torch);
        }
        if (rng.Next(2) == 0)
        {
            c.Neighbourhood.SetBlock(_center + new Vector3D<int>(3, -1, -3), Blocks.Torch);
        }
    }
}