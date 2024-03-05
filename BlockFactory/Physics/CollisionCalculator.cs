using BlockFactory.Block_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Physics;

public static class CollisionCalculator
{
    [ThreadStatic]
    private static List<Box3D<double>>? _boxes;
    
    private static List<Box3D<double>> Boxes => _boxes ??= new List<Box3D<double>>();
    
    public static (Vector3D<double>, int)? AdjustMovementForCollision(Vector3D<double> movement, Box3D<double> movingBox,
        IBlockAccess access)
    {
        var broadphase = movingBox;
        broadphase = broadphase.GetInflated(movingBox.Min + movement);
        broadphase = broadphase.GetInflated(movingBox.Max + movement);
        // broadphase.Inflate(broadphase.Min - (1.0f, 1.0f, 1.0f));
        // broadphase.Inflate(broadphase.Max + (1.0f, 1.0f, 1.0f));
        for (var x = (int)Math.Floor(broadphase.Min.X); x < (int)Math.Ceiling(broadphase.Max.X); ++x)
        for (var y = (int)Math.Floor(broadphase.Min.Y); y < (int)Math.Ceiling(broadphase.Max.Y); ++y)
        for (var z = (int)Math.Floor(broadphase.Min.Z); z < (int)Math.Ceiling(broadphase.Max.Z); ++z)
        {
            var pos = new Vector3D<int>(x, y, z);
            if (!access.IsBlockLoaded(pos))
            {
                return null;
            }

            var block = access.GetBlock(pos);
            if(block == 0 || block == Blocks.Water.Id) continue;
            Boxes.Add(new Box3D<double>(x, y, z, x + 1, y + 1, z + 1));
        }

        var res = CollisionMath.AdjustMovementForCollision(movement, movingBox, Boxes);
        Boxes.Clear();
        return res;
    }
}