using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Physics;

public static class RayCaster
{
    private static List<Box3> _boxes = new List<Box3>();
    public static (float, Direction)? GetIntersectionTime(Vector3 rayOrigin, Vector3 ray, Box3 box)
        {
            float entryTime = -1e9f, exitTime = 1e9f;
            Direction entryDirection = Direction.Down;
            for (int axis = 0; axis < 3; ++axis)
            {
                if (ray[axis] == 0f)
                {
                    if ((rayOrigin[axis] >= (box.Max[axis] - 1e-5f)) || (rayOrigin[axis] <= (box.Min[axis]) + 1e-5f))
                    {
                        return null;
                    }
                }
                else
                {
                    float t1 = (box.Min[axis] - rayOrigin[axis]) / ray[axis];
                    float t2 = (box.Max[axis] - rayOrigin[axis]) / ray[axis];
                    float currentEntryTime = MathF.Min(t1, t2);
                    float currentExitTime = MathF.Max(t1, t2);
                    if (entryTime < currentEntryTime)
                    {
                        entryTime = currentEntryTime;
                        entryDirection = DirectionUtils.FromAxisAndSign(axis, -MathF.Sign(ray[axis]));
                    }
                    if (exitTime > currentExitTime)
                    {
                        exitTime = currentExitTime;
                    }
                }
            }
            if ((entryTime <= exitTime + 1e-5f) && entryTime is >= -1e-5f and <= 1 + 1e-5f/*&& (exitTime <= 1f + (1e-5f))*/)
            {
                return (MathF.Max(MathF.Min(entryTime, 1f), 0f), entryDirection);
            }
            else
            {
                return null;
            }
        }

        public static (Vector3i, float, Direction)? RayCastBlocks(EntityPos rayOrigin, Vector3 ray, IBlockReader world)
        {
            CubeRotation curRotation = null!;
            PhysicsEntity.BoxConsumer consumer = b => { _boxes.Add(curRotation.RotateAroundCenter(b)); };
            int length = (int)MathF.Ceiling(ray.Length);
            Vector3 v = ray / length;
            Vector3i p = (-1, -1, -1);
            float f = 1e9f;
            bool hasVal = false;
            Direction d = Direction.Down;
            for (int i = 0; i < 2 * length; ++i)
            {
                Vector3 currentOffsetFromOrigin = v * (((float)i) / 2);
                EntityPos p2 = rayOrigin + currentOffsetFromOrigin;
                Vector3i blockPosL = p2.GetBlockPos();
                foreach (Direction dir in DirectionUtils.GetValues())
                {
                    Vector3i blockPos = blockPosL + dir.GetOffset();
                    BlockState state = world.GetBlockState(blockPos);
                    curRotation = state.Rotation;
                    state.Block.AddCollisionBoxes(world, blockPos, state, consumer, null!);
                    if (_boxes.Count > 0)
                    {
                        EntityPos blockFPPos = new EntityPos(blockPos);
                        Vector3 blockOffset = (rayOrigin - blockFPPos).GetAbsolutePos();
                        foreach (var box in _boxes)
                        {
                            (float, Direction)? x = GetIntersectionTime(blockOffset, ray, box);
                            if (x.HasValue)
                            {
                                (float, Direction) a = x.Value;
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
                    Vector3i blockPos = blockPosL;
                    BlockState state = world.GetBlockState(blockPos);
                    curRotation = state.Rotation;
                    state.Block.AddCollisionBoxes(world, blockPos, state, consumer, null!);
                    if (_boxes.Count > 0)
                    {
                        EntityPos blockFPPos = new EntityPos(blockPos);
                        Vector3 blockOffset = (rayOrigin - blockFPPos).GetAbsolutePos();
                        foreach (var box in _boxes)
                        {
                            (float, Direction)? x = GetIntersectionTime(blockOffset, ray, box);
                            if (x.HasValue)
                            {
                                (float, Direction) a = x.Value;
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
            {
                return (p, f, d);
            }
            else
            {
                return null;
            }
        }
}