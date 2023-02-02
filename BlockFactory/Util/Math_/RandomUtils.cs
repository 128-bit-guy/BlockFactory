using OpenTK.Mathematics;

namespace BlockFactory.Util.Math_;

public static class RandomUtils
{
    public static Vector3 PointOnSphere(Random random)
    {
        while (true)
        {
            Vector3 vec;
            vec.X = (float)random.NextDouble() * 2 - 1;
            vec.Y = (float)random.NextDouble() * 2 - 1;
            vec.Z = (float)random.NextDouble() * 2 - 1;
            if (vec.LengthSquared is <= 1.0f and >= 1e-6f) return vec.Normalized();
        }
    }
}