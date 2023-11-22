using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Physics;

public static class RayCaster
{
    public static (double, CubeFace)? GetIntersectionTime(Vector3D<double> rayOrigin, Vector3D<double> ray, Box3D<double> box)
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
        for (var i = 0; i <= length; ++i)
        {
            var curRayPos = rayOrigin + (ray * i) / length;
            var curBlockPos = curRayPos.Floor();
            foreach (var neighborFace in CubeFaceUtils.Values())
            {
                var pos = curBlockPos + neighborFace.GetDelta();
                if(world.GetBlock(pos) == 0) continue;
                var box = new Box3D<double>(pos.As<double>(), (pos + Vector3D<int>.One).As<double>());
                var intersection = GetIntersectionTime(rayOrigin, ray, box);
                if(!intersection.HasValue) continue;
                var (time, face) = intersection.Value;
                if(time >= minTime) continue;
                minTime = time;
                minPos = pos;
                minFace = face;
                found = true;
            }
            {
                var pos = curBlockPos;
                if(world.GetBlock(pos) == 0) continue;
                var box = new Box3D<double>(pos.As<double>(), (pos + Vector3D<int>.One).As<double>());
                var intersection = GetIntersectionTime(rayOrigin, ray, box);
                if(!intersection.HasValue) continue;
                var (time, face) = intersection.Value;
                if(time >= minTime) continue;
                minTime = time;
                minPos = pos;
                minFace = face;
                found = true;}
        }

        if (found)
        {
            return (minPos, minFace);
        }

        return null;
    }
}