using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace BlockFactory.CubeMath;

public static class CubeFaceUtils
{
    private static readonly CubeFace[] AllValues = Enum.GetValues<CubeFace>();


    public static CubeFace[] Values()
    {
        return AllValues;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAxis(this CubeFace face)
    {
        return (int)face >> 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSign(this CubeFace face)
    {
        return 1 - (((int)face & 1) << 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D<int> GetDelta(this CubeFace face)
    {
        var res = Vector3D<int>.Zero;
        res.SetValue(face.GetAxis(), face.GetSign());
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CubeFace GetOpposite(this CubeFace face)
    {
        return (CubeFace)((int)face ^ 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CubeFace FromAxisAndSign(int axis, int sign)
    {
        var r = (CubeFace)(axis << 1);
        if (sign < 0) r += 1;
        return r;
    }
}