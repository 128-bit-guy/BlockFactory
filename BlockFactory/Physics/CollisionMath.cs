using BlockFactory.CubeMath;
using BlockFactory.Utils;
using Silk.NET.Maths;

namespace BlockFactory.Physics;

public class CollisionMath
{
    public const double Eps = 1e-4f;
    [ThreadStatic] private static List<(int, double, double, int)>? _collisionResults;

    private static List<(int, double, double, int)> CollisionResults =>
        _collisionResults ??= new List<(int, double, double, int)>();

    public static (Vector3D<double>, int) AdjustMovementForCollision(Vector3D<double> movement, Box3D<double> movingBox,
        List<Box3D<double>> staticBoxes)
    {
        Vector3D<double> fullOkMovement = new(0, 0, 0);
        var fullLeftMovement = movement;
        var fullCollidedMask = 0;
        while (true)
        {
            var movingBoxCenter = (movingBox.Add(fullOkMovement).Min + movingBox.Add(fullOkMovement).Max) / 2.0f;
            foreach (var bb in staticBoxes)
            {
                var center = (bb.Min + bb.Max) / 2.0d;
                var (wasCollision, collisionTime, collisionAxis) =
                    Collide(fullLeftMovement, movingBox.Add(fullOkMovement), bb);
                if (wasCollision)
                    CollisionResults.Add(((int)Math.Floor(collisionTime / Eps),
                        Vector3D.Dot(center - movingBoxCenter, center - movingBoxCenter), collisionTime, collisionAxis));
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
                leftMovement.SetValue(collisionAxis, 0.0f);
                fullCollidedMask |= 1 << collisionAxis;
                fullOkMovement += okMovement;
                fullLeftMovement = leftMovement;
                CollisionResults.Clear();
            }
        }
    }


    public static (bool, double, int) Collide(Vector3D<double> movement, Box3D<double> movingBox, Box3D<double> staticBox)
    {
        var maxEntryTime = double.NegativeInfinity;
        var maxEntryAxis = 0;
        var minExitTime = double.PositiveInfinity;
        for (var i = 0; i < 3; ++i)
        {
            double currentEntryTime;
            double currentExitTime;
            if (movement[i] == 0.0f)
            {
                currentExitTime = double.PositiveInfinity;
                if (staticBox.Min[i] < movingBox.Max[i] && staticBox.Max[i] > movingBox.Min[i])
                    currentEntryTime = double.NegativeInfinity;
                else
                    currentEntryTime = double.PositiveInfinity;
            }
            else
            {
                var movement0 = staticBox.Min[i] - movingBox.Max[i];
                var movement1 = staticBox.Max[i] - movingBox.Min[i];
                var time0 = movement0 / movement[i];
                var time1 = movement1 / movement[i];
                currentEntryTime = Math.Min(time0, time1);
                currentExitTime = Math.Max(time0, time1);
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
        return (true, Math.Max(maxEntryTime - Eps, -Eps), maxEntryAxis);
    }
}