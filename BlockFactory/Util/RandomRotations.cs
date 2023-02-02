using BlockFactory.CubeMath;

namespace BlockFactory.Util;

public static class RandomRotations
{
    public static CubeRotation Any(Random rng)
    {
        return CubeRotation.Rotations[rng.Next(CubeRotation.Rotations.Length)];
    }

    public static CubeRotation KeepingUp(Random rng)
    {
        return CubeRotation.GetFromTo(Direction.Up, Direction.Up)[rng.Next(4)];
    }

    public static CubeRotation KeepingY(Random rng)
    {
        var up = rng.Next(2) == 0;
        return CubeRotation.GetFromTo(Direction.Up, up
            ? Direction.Up
            : Direction.Down)[rng.Next(4)];
    }
}