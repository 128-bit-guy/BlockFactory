using System.Drawing;
using System.Runtime.CompilerServices;
using BlockFactory.CubeMath;
using Silk.NET.Maths;

namespace BlockFactory.Utils;

public static class BfMathUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<T> ShiftRight<T>(this Vector3D<T> vec, int offset) where T : unmanaged, IFormattable,
        IEquatable<T>, IComparable<T>
    {
        return new Vector3D<T>(Scalar.ShiftRight(vec.X, offset), Scalar.ShiftRight(vec.Y, offset),
            Scalar.ShiftRight(vec.Z, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<T> ShiftLeft<T>(this Vector3D<T> vec, int offset) where T : unmanaged, IFormattable,
        IEquatable<T>, IComparable<T>
    {
        return new Vector3D<T>(Scalar.ShiftLeft(vec.X, offset), Scalar.ShiftLeft(vec.Y, offset),
            Scalar.ShiftLeft(vec.Z, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector4D<float> AsVector(this Color color)
    {
        return new Vector4D<float>(color.R / (float)byte.MaxValue, color.G / (float)byte.MaxValue,
            color.B / (float)byte.MaxValue, color.A / (float)byte.MaxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Lerp(float f, float a, float b)
    {
        return (1 - f) * a + f * b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box3D<T> Add<T>(this Box3D<T> a, Vector3D<T> b)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Box3D<T>(a.Min + b, a.Max + b);
    }
    
    public static Vector3D<float> Unproject(
        Vector3D<float> vector,
        float x,
        float y,
        float width,
        float height,
        float minZ,
        float maxZ,
        Matrix4X4<float> inverseWorldViewProjection)
    {
        float num1 = (float) (((double) vector.X - (double) x) / (double) width * 2.0 - 1.0);
        float num2 = (float) (((double) vector.Y - (double) y) / (double) height * 2.0 - 1.0);
        float num3 = (float) (((double) vector.Z - (double) minZ) / ((double) maxZ - (double) minZ) * 2.0 - 1.0);
        Vector3D<float> vector3;
        vector3.X = (float) ((double) num1 * (double) inverseWorldViewProjection.M11 + (double) num2 * (double) inverseWorldViewProjection.M21 + (double) num3 * (double) inverseWorldViewProjection.M31) + inverseWorldViewProjection.M41;
        vector3.Y = (float) ((double) num1 * (double) inverseWorldViewProjection.M12 + (double) num2 * (double) inverseWorldViewProjection.M22 + (double) num3 * (double) inverseWorldViewProjection.M32) + inverseWorldViewProjection.M42;
        vector3.Z = (float) ((double) num1 * (double) inverseWorldViewProjection.M13 + (double) num2 * (double) inverseWorldViewProjection.M23 + (double) num3 * (double) inverseWorldViewProjection.M33) + inverseWorldViewProjection.M43;
        float num4 = (float) ((double) num1 * (double) inverseWorldViewProjection.M14 + (double) num2 * (double) inverseWorldViewProjection.M24 + (double) num3 * (double) inverseWorldViewProjection.M34) + inverseWorldViewProjection.M44;
        return vector3 / num4;
    }

    public static Vector3D<T> BoxLerp<T>(Box3D<T> box, Vector3D<T> vec) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        Vector3D<T> res = default;
        for (var i = 0; i < 3; ++i)
        {
            res.SetValue(i, Scalar.Add(
                Scalar.Multiply(Scalar.Subtract(Scalar<T>.One, vec[i]), box.Min[i]),
                Scalar.Multiply(vec[i], box.Max[i])
                ));
        }

        return res;
    }

    public static float SoftSign(float x)
    {
        return MathF.Atan(x) / (MathF.PI / 2);
    }
}