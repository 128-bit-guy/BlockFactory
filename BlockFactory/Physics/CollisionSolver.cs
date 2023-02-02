using BlockFactory.CubeMath;
using OpenTK.Mathematics;

namespace BlockFactory.Physics;

public class CollisionSolver
{
    public const float Eps = 1e-4f;
    private static readonly List<(int, float, float, int)> CollisionResults = new();

    public static (Vector3, int) AdjustMovementForCollision(Vector3 movement, Box3 movingBox,
        List<Box3> staticBoxes)
    {
        Vector3 fullOkMovement = (0, 0, 0);
        var fullLeftMovement = movement;
        var fullCollidedMask = 0;
        while (true)
        {
            var movingBoxCenter = (movingBox.Add(fullOkMovement).Min + movingBox.Add(fullOkMovement).Max) / 2.0f;
            foreach (var bb in staticBoxes)
            {
                var center = (bb.Min + bb.Max) / 2.0f;
                var (wasCollision, collisionTime, collisionAxis) =
                    Collide(fullLeftMovement, movingBox.Add(fullOkMovement), bb);
                if (wasCollision)
                    CollisionResults.Add(((int)MathF.Floor(collisionTime / Eps),
                        Vector3.Dot(center - movingBoxCenter, center - movingBoxCenter), collisionTime, collisionAxis));
            }

            if (CollisionResults.Count == 0)
            {
                CollisionResults.Clear();
                return (fullOkMovement + fullLeftMovement, fullCollidedMask);
            }

            {
                var (_, _, collisionTime, collisionAxis) = CollisionResults.Min();
                var okMovement = fullLeftMovement * collisionTime;
                var leftMovement = fullLeftMovement - okMovement;
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
        var maxEntryTime = float.NegativeInfinity;
        var maxEntryAxis = 0;
        var minExitTime = float.PositiveInfinity;
        for (var i = 0; i < 3; ++i)
        {
            float currentEntryTime;
            float currentExitTime;
            if (movement[i] == 0.0f)
            {
                currentExitTime = float.PositiveInfinity;
                if (staticBox.Min[i] < movingBox.Max[i] && staticBox.Max[i] > movingBox.Min[i])
                    currentEntryTime = float.NegativeInfinity;
                else
                    currentEntryTime = float.PositiveInfinity;
            }
            else
            {
                var movement0 = staticBox.Min[i] - movingBox.Max[i];
                var movement1 = staticBox.Max[i] - movingBox.Min[i];
                var time0 = movement0 / movement[i];
                var time1 = movement1 / movement[i];
                currentEntryTime = MathF.Min(time0, time1);
                currentExitTime = MathF.Max(time0, time1);
            }

            if (currentEntryTime > maxEntryTime)
            {
                maxEntryTime = currentEntryTime;
                maxEntryAxis = i;
            }

            if (currentExitTime < minExitTime) minExitTime = currentExitTime;
        }

        if (maxEntryTime > minExitTime) return (false, 0.0f, 0);
        if (maxEntryTime < -Eps) return (false, 0.0f, 0);
        if (maxEntryTime > 1.0f + Eps) return (false, 0.0f, 0);
        return (true, MathF.Max(maxEntryTime - Eps, -Eps), maxEntryAxis);
    }
}