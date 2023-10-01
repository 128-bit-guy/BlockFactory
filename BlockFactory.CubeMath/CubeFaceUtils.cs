using Silk.NET.Maths;

namespace BlockFactory.CubeMath;

public static class CubeFaceUtils
{
    private static readonly CubeFace[] AllValues = Enum.GetValues<CubeFace>();
    

    public static CubeFace[] Values()
    {
        return AllValues;
    }

    public static int GetAxis(this CubeFace face)
    {
        return (int)face >> 1;
    }

    public static int GetSign(this CubeFace face)
    {
        return 1 - (((int)face & 1) << 1);
    }

    public static Vector3D<int> GetDelta(this CubeFace face)
    {
        var res = Vector3D<int>.Zero;
        res.SetValue(face.GetAxis(), face.GetSign());
        return res;
    }
}