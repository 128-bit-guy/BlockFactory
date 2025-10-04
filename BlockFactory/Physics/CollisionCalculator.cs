using BlockFactory.Content.Block_;
using BlockFactory.World_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Physics;

public static class CollisionCalculator
{
    [ThreadStatic]
    private static BoxConsumer? _boxes;
    
    private static BoxConsumer Boxes => _boxes ??= new BoxConsumer();
    
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

            var block = access.GetBlockObj(pos);
            Boxes.Offset = new Vector3D<double>(x, y, z);
            block.AddBlockBoxes(new ConstBlockPointer(access, pos), Boxes.Func, BlockBoxType.Collision);
        }

        var res = CollisionMath.AdjustMovementForCollision(movement, movingBox, Boxes.Boxes);
        Boxes.Clear();
        return res;
    }
}