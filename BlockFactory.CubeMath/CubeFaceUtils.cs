using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace BlockFactory.CubeMath;

public static class CubeFaceUtils
{
    private static readonly CubeFace[] Values = (Enum.GetValues(typeof(CubeFace)) as CubeFace[])!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAxis(this CubeFace face)
    {
        return (int)face >> 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSign(this CubeFace face)
    {
        return ((((int)face & 1) ^ 1) << 1) - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CubeFace GetOpposite(this CubeFace face)
    {
        return (CubeFace)((int)face ^ 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i GetOffset(this CubeFace face)
    {
        Vector3i result = (0, 0, 0);
        result[face.GetAxis()] = face.GetSign();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CubeFace[] GetValues()
    {
        return Values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNextAxis(int axis)
    {
        return (axis + 1) % 3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetPrevAxis(int axis)
    {
        return (axis - 1 + 3) % 3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CubeFace FromAxisAndSign(int axis, int sign)
    {
        var r = (CubeFace)(axis << 1);
        if (sign < 0) r += 1;
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CubeFace FromVector(Vector3 v)
    {
        float ma = -1;
        var f = CubeFace.Back;
        foreach (var face in GetValues())
        {
            var c = Vector3.Dot(face.GetOffset(), v);
            if (c > ma)
            {
                ma = c;
                f = face;
            }
        }

        return f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CubeFace FromVector(Vector3i v)
    {
        var ma = -1;
        var f = CubeFace.Back;
        foreach (var face in GetValues())
        {
            var c = face.GetOffset().Dot(v);
            if (c > ma)
            {
                ma = c;
                f = face;
            }
        }

        return f;
    }
}