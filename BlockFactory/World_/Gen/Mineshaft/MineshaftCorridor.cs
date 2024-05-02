using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen.Mineshaft;

public class MineshaftCorridor : IMineshaftElement
{
    private readonly Vector3D<int> _center;
    private readonly CubeFace _direction;
    private readonly CubeSymmetry _symmetry;

    public MineshaftCorridor(Vector3D<int> center, CubeFace direction)
    {
        _center = center;
        _direction = direction;
        _symmetry = CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, _direction, CubeFace.Top)!;
    }

    public Vector3D<int> GetCenter()
    {
        return _center;
    }

    public void AddNeighbours(Random rng, Queue<IMineshaftElement> elements)
    {
        int comp = rng.Next(20);
        var ladderRoom = comp == 0;
        var room = comp is  >= 1 and <= 3;
        var relPos = room ? new Vector3D<int>(0, 0, 10) :
            ladderRoom ? new Vector3D<int>(0, 0, 9) : new Vector3D<int>(0, 0, 7);
        if (!ladderRoom && rng.Next(5) == 0)
        {
            relPos.Y += rng.Next(2) == 0 ? -1 : 1;
        }

        var absPos = _center + relPos * _symmetry;
        if (room)
        {
            elements.Enqueue(new MineshaftRoom(absPos, _direction.GetOpposite()));
        }
        else if (ladderRoom)
        {
            elements.Enqueue(new MineshaftLadderRoom(absPos, _direction.GetOpposite()));
        }
        else
        {
            elements.Enqueue(new MineshaftCorridor(absPos, _direction));
        }
    }

    private short GetBlock(Chunk c, Vector3D<int> relPos)
    {
        return c.Neighbourhood.GetBlock(_center + relPos * _symmetry);
    }

    private void SetBlock(Chunk c, Vector3D<int> relPos, int block)
    {
        c.Neighbourhood.SetBlock(_center + relPos * _symmetry, (short)block);
    }

    public void Generate(Chunk c, Random rng)
    {
        var s = CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, _direction, CubeFace.Top)!;

        for (var i = -1; i <= 1; ++i)
        {
            for (var k = 0; k <= 6; ++k)
            {
                for (var j = -1; j <= 1; ++j)
                {
                    SetBlock(c, new Vector3D<int>(i, j, k), 0);
                }

                {
                    var b = GetBlock(c, new Vector3D<int>(i, -2, k));
                    if (b == 0 || b == Blocks.Water.Id)
                    {
                        SetBlock(c, new Vector3D<int>(i, -2, k), Blocks.Planks.Id);
                    }
                }
            }

            SetBlock(c, new Vector3D<int>(i, 1, 3), Blocks.Planks.Id);
        }

        SetBlock(c, new Vector3D<int>(-1, 0, 3), Blocks.Fence.Id);
        SetBlock(c, new Vector3D<int>(1, 0, 3), Blocks.Fence.Id);
        SetBlock(c, new Vector3D<int>(-1, -1, 3), Blocks.Fence.Id);
        SetBlock(c, new Vector3D<int>(1, -1, 3), Blocks.Fence.Id);
        if (rng.Next(2) == 0)
        {
            SetBlock(c, new Vector3D<int>(0, 1, 2), Blocks.Torch.Id);
        }
        if (rng.Next(2) == 0)
        {
            SetBlock(c, new Vector3D<int>(0, 1, 4), Blocks.Torch.Id);
        }
    }
}