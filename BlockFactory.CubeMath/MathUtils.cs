using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace BlockFactory.CubeMath;

public static class MathUtils
{
    public static void SetValue<T>(this ref Vector3D<T> vec, int index, T val) where T : unmanaged, IFormattable,
        IEquatable<T>, IComparable<T>
    {
        Unsafe.Add(ref vec.X, index) = val;
    }
    public static void SetValue<T>(this ref Matrix4X4<T> mat, int i, int j, T val) where T : unmanaged, IFormattable,
        IEquatable<T>, IComparable<T>
    {
        Unsafe.Add(ref Unsafe.Add(ref mat.Row1, i).X, j) = val;
    }
}