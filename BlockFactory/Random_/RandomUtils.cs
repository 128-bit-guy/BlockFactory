using Silk.NET.Maths;

namespace BlockFactory.Random_;

public static class RandomUtils
{
    public static Vector3D<float> PointOnSphere(Random random)
    {
        while (true)
        {
            Vector3D<float> vec;
            vec.X = (float)random.NextDouble() * 2 - 1;
            vec.Y = (float)random.NextDouble() * 2 - 1;
            vec.Z = (float)random.NextDouble() * 2 - 1;
            if (vec.LengthSquared is <= 1.0f and >= 1e-6f) return Vector3D.Normalize(vec);
        }
    }
}