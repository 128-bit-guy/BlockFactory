using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Physics;

public static class RayCaster
{
    private static readonly List<Box3> _boxes = new();

    public static (float, Direction)? GetIntersectionTime(Vector3 rayOrigin, Vector3 ray, Box3 box)
    {
        float entryTime = -1e9f, exitTime = 1e9f;
        var entryDirection = Direction.Down;
        for (var axis = 0; axis < 3; ++axis)
            if (ray[axis] == 0f)
            {
                if (rayOrigin[axis] >= box.Max[axis] - 1e-5f || rayOrigin[axis] <= box.Min[axis] + 1e-5f) return null;
            }
            else
            {
                var t1 = (box.Min[axis] - rayOrigin[axis]) / ray[axis];
                var t2 = (box.Max[axis] - rayOrigin[axis]) / ray[axis];
                var currentEntryTime = MathF.Min(t1, t2);
                var currentExitTime = MathF.Max(t1, t2);
                if (entryTime < currentEntryTime)
                {
                    entryTime = currentEntryTime;
                    entryDirection = DirectionUtils.FromAxisAndSign(axis, -MathF.Sign(ray[axis]));
                }

                if (exitTime > currentExitTime) exitTime = currentExitTime;
            }

        if (entryTime <= exitTime + 1e-5f && entryTime is >= -1e-5f and <= 1 + 1e-5f /*&& (exitTime <= 1f + (1e-5f))*/)
            return (MathF.Max(MathF.Min(entryTime, 1f), 0f), entryDirection);
        return null;
    }

    public static (Vector3i, float, Direction)? RayCastBlocks(EntityPos rayOrigin, Vector3 ray, IBlockReader world)
    {
        CubeRotation curRotation = null!;
        PhysicsEntity.BoxConsumer consumer = b => { _boxes.Add(curRotation.RotateAroundCenter(b)); };
        var length = (int)MathF.Ceiling(ray.Length);
        var v = ray / length;
        Vector3i p = (-1, -1, -1);
        var f = 1e9f;
        var hasVal = false;
        var d = Direction.Down;
        for (var i = 0; i < 2 * length; ++i)
        {
            var currentOffsetFromOrigin = v * ((float)i / 2);
            var p2 = rayOrigin + currentOffsetFromOrigin;
            var blockPosL = p2.GetBlockPos();
            foreach (var dir in DirectionUtils.GetValues())
            {
                var blockPos = blockPosL + dir.GetOffset();
                var state = world.GetBlockState(blockPos);
                curRotation = state.Rotation;
                state.Block.AddCollisionBoxes(world, blockPos, state, consumer, null!);
                if (_boxes.Count > 0)
                {
                    var blockFPPos = new EntityPos(blockPos);
                    var blockOffset = (rayOrigin - blockFPPos).GetAbsolutePos();
                    foreach (var box in _boxes)
                    {
                        var x = GetIntersectionTime(blockOffset, ray, box);
                        if (x.HasValue)
                        {
                            var a = x.Value;
                            if (a.Item1 < f)
                            {
                                hasVal = true;
                                f = a.Item1;
                                p = blockPos;
                                d = a.Item2;
                            }
                        }
                    }

                    _boxes.Clear();
                }
            }

            {
                var blockPos = blockPosL;
                var state = world.GetBlockState(blockPos);
                curRotation = state.Rotation;
                state.Block.AddCollisionBoxes(world, blockPos, state, consumer, null!);
                if (_boxes.Count > 0)
                {
                    var blockFPPos = new EntityPos(blockPos);
                    var blockOffset = (rayOrigin - blockFPPos).GetAbsolutePos();
                    foreach (var box in _boxes)
                    {
                        var x = GetIntersectionTime(blockOffset, ray, box);
                        if (x.HasValue)
                        {
                            var a = x.Value;
                            if (a.Item1 < f)
                            {
                                hasVal = true;
                                f = a.Item1;
                                p = blockPos;
                                d = a.Item2;
                            }
                        }
                    }

                    _boxes.Clear();
                }
            }
        }

        if (hasVal)
            return (p, f, d);
        return null;
    }
}