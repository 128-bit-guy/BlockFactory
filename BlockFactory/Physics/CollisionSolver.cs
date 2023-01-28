using BlockFactory.CubeMath;
using OpenTK.Mathematics;
using BlockFactory.Util.Math_;

namespace BlockFactory.Physics;

public class CollisionSolver
{
    public const float Eps = 1e-4f;
        private static List<(int, float, float, int)> CollisionResults = new List<(int, float, float, int)>();
        public static (Vector3, int) AdjustMovementForCollision(Vector3 movement, Box3 movingBox,
                                                List<Box3> staticBoxes)
        {
            Vector3 fullOkMovement = (0, 0, 0);
            Vector3 fullLeftMovement = movement;
            int fullCollidedMask = 0;
            while (true)
            {
                Vector3 movingBoxCenter = (movingBox.Add(fullOkMovement).Min + movingBox.Add(fullOkMovement).Max) / 2.0f;
                foreach (Box3 bb in staticBoxes)
                {
                    Vector3 center = (bb.Min + bb.Max) / 2.0f;
                    var (wasCollision, collisionTime, collisionAxis) = Collide(fullLeftMovement, movingBox.Add(fullOkMovement), bb);
                    if (wasCollision)
                    {
                        CollisionResults.Add((((int)MathF.Floor(collisionTime / Eps)), Vector3.Dot(center - movingBoxCenter, center - movingBoxCenter), collisionTime, collisionAxis));
                    }
                }
                if (CollisionResults.Count == 0)
                {
                    CollisionResults.Clear();
                    return (fullOkMovement + fullLeftMovement, fullCollidedMask);
                }
                else
                {
                    var (_, _, collisionTime, collisionAxis) = CollisionResults.Min();
                    Vector3 okMovement = fullLeftMovement * collisionTime;
                    Vector3 leftMovement = fullLeftMovement - okMovement;
                    leftMovement[collisionAxis] = 0.0f;
                    fullCollidedMask |= 1 << collisionAxis;
                    fullOkMovement += okMovement;
                    fullLeftMovement = leftMovement;
                    CollisionResults.Clear();
                }
            }
        }


        public static (bool, float, int) Collide(Vector3 movement, Box3 movingBox, Box3 staticBox)
        {
            float maxEntryTime = float.NegativeInfinity;
            int maxEntryAxis = 0;
            float minExitTime = float.PositiveInfinity;
            for (int i = 0; i < 3; ++i)
            {
                float currentEntryTime;
                float currentExitTime;
                if (movement[i] == 0.0f)
                {
                    currentExitTime = float.PositiveInfinity;
                    if ((staticBox.Min[i] < movingBox.Max[i]) && (staticBox.Max[i] > movingBox.Min[i]))
                    {
                        currentEntryTime = float.NegativeInfinity;
                    }
                    else
                    {
                        currentEntryTime = float.PositiveInfinity;
                    }
                }
                else
                {
                    float movement0 = staticBox.Min[i] - movingBox.Max[i];
                    float movement1 = staticBox.Max[i] - movingBox.Min[i];
                    float time0 = movement0 / movement[i];
                    float time1 = movement1 / movement[i];
                    currentEntryTime = MathF.Min(time0, time1);
                    currentExitTime = MathF.Max(time0, time1);
                }
                if (currentEntryTime > maxEntryTime)
                {
                    maxEntryTime = currentEntryTime;
                    maxEntryAxis = i;
                }
                if (currentExitTime < minExitTime)
                {
                    minExitTime = currentExitTime;
                }
            }
            if (maxEntryTime > minExitTime)
            {
                return (false, 0.0f, 0);
            }
            if (maxEntryTime < -Eps)
            {
                return (false, 0.0f, 0);
            }
            if (maxEntryTime > 1.0f + Eps)
            {
                return (false, 0.0f, 0);
            }
            return (true, MathF.Max(maxEntryTime - Eps, -Eps), maxEntryAxis);
        }
}