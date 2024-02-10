﻿using BlockFactory.Block_;
using BlockFactory.Random_;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public static class TreeGenerator
{
    public static void Generate(BlockPointer pointer, Random rng)
    {
        Span<Vector3D<int>> neighbours = stackalloc Vector3D<int>[4];
        neighbours[0] = Vector3D<int>.UnitX;
        neighbours[1] = Vector3D<int>.UnitZ;
        neighbours[2] = -Vector3D<int>.UnitX;
        neighbours[3] = -Vector3D<int>.UnitZ;
        neighbours.Shuffle(rng);
        GenerateBranch(pointer, Vector3D<float>.UnitY, Vector3D<float>.Zero, rng, 25, neighbours);
        for (var i = 0; i < 4; ++i)
        {
            var neighbour = pointer + neighbours[i] - Vector3D<int>.UnitY;
            for (var j = 0; j < 3; ++j)
            {
                var neighbourBottom = neighbour - new Vector3D<int>(0, j, 0);
                if (neighbourBottom.GetBlock() == 0 || neighbourBottom.GetBlock() == Blocks.Leaves.Id)
                {
                    neighbourBottom.SetBlock(Blocks.Log);
                }
                else
                {
                    break;
                }
            }
        }
    }

    private static void GenerateBranch(BlockPointer pointer, Vector3D<float> direction, Vector3D<float> relPos,
        Random rng, int leftLength, Span<Vector3D<int>> neighbours)
    {
        var first = true;
        while (leftLength > 0)
        {
            if (first)
            {
                first = false;
            }
            else if (rng.Next(80) < leftLength)
            {
                GenerateBranch(pointer, direction, relPos, rng, leftLength, neighbours);
            }

            var np = pointer + new Vector3D<float>(MathF.Round(relPos.X), MathF.Round(relPos.Y),
                MathF.Round(relPos.Z)).As<int>();
            for (int k = 0; k < 2; ++k)
            {
                var np2 = np + new Vector3D<int>(0, k, 0);
                np2.SetBlock(Blocks.Log);
                for (int j = 0; j < Math.Min(4, (leftLength - 12) / 2 - 1); ++j)
                {
                    (np2 + neighbours[j]).SetBlock(Blocks.Log);
                }
            }

            if (leftLength < 3)
            {
                SetLeaves(np, 4);
            }

            relPos += direction / 2;
            var newDir = 4 * direction + RandomUtils.PointOnSphere(rng) + 0 * Vector3D<float>.UnitY;
            direction = Vector3D.Normalize(newDir);
            --leftLength;
        }
    }

    private static void SetLeaves(BlockPointer center, int radius)
    {
        for (var i = -radius; i <= radius; ++i)
        for (var j = -radius; j <= radius; ++j)
        for (var k = -radius; k <= radius; ++k)
        {
            var delta = new Vector3D<int>(i, j, k);
            if (delta.LengthSquared > radius * radius) continue;
            var leafPos = center + delta;
            if (leafPos.GetBlock() == 0)
            {
                leafPos.SetBlock(Blocks.Leaves);
            }
        }
    }
}