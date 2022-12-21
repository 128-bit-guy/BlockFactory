using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace BlockFactory.CubeMath;

public static class MathUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ManhattanLength(this Vector3i v)
    {
        return Math.Abs(v.X) + Math.Abs(v.Y) + Math.Abs(v.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ManhattanLength(this Vector2i v)
    {
        return Math.Abs(v.X) + Math.Abs(v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SquareLength(this Vector3i v)
    {
        return Square(v.X) + Square(v.Y) + Square(v.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SquareLength(this Vector2i v)
    {
        return Square(v.X) + Square(v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Square(int a)
    {
        return a * a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MaxComponent(this Vector3i v)
    {
        return Math.Max(Math.Abs(v.X), Math.Max(Math.Abs(v.Y), Math.Abs(v.Z)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MaxComponent(this Vector2i v)
    {
        return Math.Max(Math.Abs(v.X), Math.Abs(v.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i BitShiftRight(this Vector2i v, int by)
    {
        return (v.X >> by, v.Y >> by);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i BitShiftLeft(this Vector2i v, int by)
    {
        return (v.X << by, v.Y << by);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i BitShiftRight(this Vector3i v, int by)
    {
        return (v.X >> by, v.Y >> by, v.Z >> by);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i BitShiftLeft(this Vector3i v, int by)
    {
        return (v.X << by, v.Y << by, v.Z << by);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i BitAnd(this Vector2i v, int with)
    {
        return (v.X & with, v.Y & with);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i BitAnd(this Vector3i v, int with)
    {
        return (v.X & with, v.Y & with, v.Z & with);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreAllComponentsBetween(this Vector3i v, int min, int max)
    {
        return v.X >= min && v.X < max && v.Y >= min && v.Y < max && v.Z >= min && v.Z < max;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetPosInLinearArray(this Vector3i v, int xSize, int ySize)
    {
        return v.X + v.Y * xSize + v.Z * xSize * ySize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetPosInLinearArray(this Vector3i v, int size)
    {
        return v.GetPosInLinearArray(size, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i Floor(this Vector3 v)
    {
        return (
            (int)MathF.Floor(v.X),
            (int)MathF.Floor(v.Y),
            (int)MathF.Floor(v.Z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToVector3(this Vector4 v)
    {
        return v.Xyz * v.W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box3 Add(this Box3 a, Vector3 b)
    {
        a.Center += b;
        return a;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Dot(this Vector3i a, Vector3i b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float pos, float p0, float p1)
    {
        return pos * p1 + (1 - pos) * p0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BiLerp(Vector2 pos, float p00, float p10, float p01, float p11)
    {
        var p0 = Lerp(pos.Y, p00, p01);
        var p1 = Lerp(pos.Y, p10, p11);
        return Lerp(pos.X, p0, p1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Apply(this Matrix4 mat, Vector3 vec)
    {
        return Vector3.TransformPosition(vec, mat);
        /*Vector4 v = mat * new Vector4(vec, 1f);
        return v.Xyz * v.W;*/
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorMod(int a, int b)
    {
        var x = a % b;
        if (x >= 0)
            return x;
        return x + b;
    }
}