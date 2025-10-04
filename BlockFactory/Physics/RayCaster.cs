using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Utils;
using BlockFactory.World_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Physics;

public static class RayCaster
{
    public static (double, CubeFace)? GetIntersectionTime(Vector3D<double> rayOrigin, Vector3D<double> ray,
        Box3D<double> box)
    {
        double entryTime = -1e9f, exitTime = 1e9f;
        var entryCubeFace = CubeFace.Bottom;
        for (var axis = 0; axis < 3; ++axis)
            if (ray[axis] == 0f)
            {
                if (rayOrigin[axis] >= box.Max[axis] - 1e-5f || rayOrigin[axis] <= box.Min[axis] + 1e-5f) return null;
            }
            else
            {
                var t1 = (box.Min[axis] - rayOrigin[axis]) / ray[axis];
                var t2 = (box.Max[axis] - rayOrigin[axis]) / ray[axis];
                var currentEntryTime = Math.Min(t1, t2);
                var currentExitTime = Math.Max(t1, t2);
                if (entryTime < currentEntryTime)
                {
                    entryTime = currentEntryTime;
                    entryCubeFace = CubeFaceUtils.FromAxisAndSign(axis, -Math.Sign(ray[axis]));
                }

                if (exitTime > currentExitTime) exitTime = currentExitTime;
            }

        if (entryTime <= exitTime + 1e-5f && entryTime is >= -1e-5f and <= 1 + 1e-5f /*&& (exitTime <= 1f + (1e-5f))*/)
            return (Math.Max(Math.Min(entryTime, 1f), 0f), entryCubeFace);
        return null;
    }

    public static (Vector3D<int>, CubeFace)? RayCastBlocks(IBlockAccess world, Vector3D<double> rayOrigin,
        Vector3D<double> ray)
    {
        var length = (int)Math.Ceiling(ray.Length);
        var found = false;
        double minTime = 1e9f;
        Vector3D<int> minPos = default;
        CubeFace minFace = default;
        Vector3D<int> pos = new Vector3D<int>();
        BoxConsumer.BoxConsumerFunc boxConsumer = box =>
        {
            var absBox = box.Add(pos.As<double>());
            var intersection = GetIntersectionTime(rayOrigin, ray, absBox);
            if (!intersection.HasValue) return;
            var (time, face) = intersection.Value;
            if (time >= minTime) return;
            minTime = time;
            minPos = pos;
            minFace = face;
            found = true;
        };
        for (var i = 0; i <= length; ++i)
        {
            var curRayPos = rayOrigin + ray * i / length;
            var curBlockPos = curRayPos.Floor();
            foreach (var neighborFace in CubeFaceUtils.Values())
            {
                pos = curBlockPos + neighborFace.GetDelta();
                world.GetBlockObj(pos)
                    .AddBlockBoxes(new ConstBlockPointer(world, pos), boxConsumer, BlockBoxType.RayCasting);
            }

            {
                pos = curBlockPos;
                world.GetBlockObj(pos)
                    .AddBlockBoxes(new ConstBlockPointer(world, pos), boxConsumer, BlockBoxType.RayCasting);
            }
        }

        if (found) return (minPos, minFace);

        return null;
    }
}