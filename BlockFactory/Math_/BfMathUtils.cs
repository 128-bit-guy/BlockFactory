﻿using System.Drawing;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace BlockFactory.Math_;

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
}