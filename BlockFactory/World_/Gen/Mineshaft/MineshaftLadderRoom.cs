using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen.Mineshaft;

public class MineshaftLadderRoom : IMineshaftElement
{
    private readonly Vector3D<int> _center;
    private readonly CubeFace _fromDir;
    private readonly bool[] _occupiedNeighbours = new bool[6];

    public MineshaftLadderRoom(Vector3D<int> center, CubeFace fromDir)
    {
        _center = center;
        _fromDir = fromDir;
        _occupiedNeighbours[(int)_fromDir] = true;
    }

    public Vector3D<int> GetCenter()
    {
        return _center;
    }

    public void AddNeighbours(Random rng, Queue<IMineshaftElement> elements)
    {
        {
            var vertOccupied = _occupiedNeighbours[(int)CubeFace.Top] || _occupiedNeighbours[(int)CubeFace.Bottom];
            var up = rng.Next(5) == 0;
            var down = rng.Next(5) == 0;
            if (!vertOccupied)
            {
                if (rng.Next(2) == 0)
                {
                    up = true;
                }
                else
                {
                    down = true;
                }
            }

            if (up)
            {
                _occupiedNeighbours[(int)CubeFace.Top] = true;
                elements.Enqueue(
                    new MineshaftLadderRoom(_center + new Vector3D<int>(0, 5, 0), CubeFace.Bottom)
                    );
            }

            if (down)
            {
                _occupiedNeighbours[(int)CubeFace.Bottom] = true;
                elements.Enqueue(
                    new MineshaftLadderRoom(_center + new Vector3D<int>(0, -5, 0), CubeFace.Top)
                );
            }
        }
        {
            Span<bool> additions = stackalloc bool[6];
            var forceAdd = !(_occupiedNeighbours[(int)CubeFace.Top] && _occupiedNeighbours[(int)CubeFace.Bottom]);
            foreach (var face in CubeFaceUtils.Values())
            {
                if(face.GetAxis() == 1) continue;
                if (_occupiedNeighbours[(int)face])
                {
                    forceAdd = false;
                    break;
                }
            }

            if (forceAdd)
            {
                var i = rng.Next(4);
                additions[i] = true;
            }

            for (var i = 0; i < 4; ++i)
            {
                additions[i] |= rng.Next(10) == 0;
            }

            for (var i = 0; i < 4; ++i)
            {
                if(!additions[i]) continue;
                var face = CubeFaceUtils.Horizontals()[i];
                if(face == _fromDir) continue;
                _occupiedNeighbours[(int)face] = true;
                var center = _center + face.GetDelta() * 3;
                elements.Enqueue(new MineshaftCorridor(center, face));
            }
        }
    }

    public void Generate(Chunk c, Random rng)
    {
        var floor = !_occupiedNeighbours[(int)CubeFace.Bottom];
        foreach (var face in CubeFaceUtils.Values())
        {
            if(face.GetAxis() == 1) continue;
            if (_occupiedNeighbours[(int)face])
            {
                floor = true;
                break;
            }
        }
        for (var i = -2; i <= 2; ++i)
        {
            for (var k = -2; k <= 2; ++k)
            {
                for (var j = _occupiedNeighbours[(int)CubeFace.Bottom]? -2 : -1; j <= 2; ++j)
                {
                    var absPos = _center + new Vector3D<int>(i, j, k);
                    c.Neighbourhood.SetBlock(absPos, 0);
                }

                if (floor) {
                    var absPos = _center + new Vector3D<int>(i, -2, k);
                    if (c.Neighbourhood.GetBlock(absPos) == 0 || c.Neighbourhood.GetBlock(absPos) == Blocks.Water.Id)
                    {
                        c.Neighbourhood.SetBlock(absPos, Blocks.Planks);
                    }
                }
            }
        }

        for (var j = _occupiedNeighbours[(int)CubeFace.Bottom] ? -2 : -1; j <= 2; ++j)
        {
            for (var i = -1; i <= 1; ++i)
            {
                for (var k = -1; k <= 1; ++k)
                {
                    var absPos = _center + new Vector3D<int>(i, j, k);
                    c.Neighbourhood.SetBlock(absPos, 0);
                }
            }
            {
                var absPos = _center + new Vector3D<int>(0, j, 0);
                c.Neighbourhood.SetBlock(absPos, Blocks.Log);
            }
        }
    }
}