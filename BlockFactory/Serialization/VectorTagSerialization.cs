using BlockFactory.CubeMath;
using Silk.NET.Maths;

namespace BlockFactory.Serialization;

public static class VectorTagSerialization
{
    public static void SetVector3D<T>(this DictionaryTag tag, string name, Vector3D<T> value)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        var array = new T[3];
        value.CopyTo(array);
        tag.SetValue(name, array);
    }

    public static Vector3D<T> GetVector3D<T>(this DictionaryTag tag, string name)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        var array = tag.GetArray<T>(name, 3);
        var res = new Vector3D<T>();
        for (var i = 0; i < 3; ++i)
        {
            res.SetValue(i, array[i]);
        }

        return res;
    }
    public static void SetVector2D<T>(this DictionaryTag tag, string name, Vector2D<T> value)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        var array = new T[2];
        value.CopyTo(array);
        tag.SetValue(name, array);
    }

    public static Vector2D<T> GetVector2D<T>(this DictionaryTag tag, string name)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        var array = tag.GetArray<T>(name, 2);
        var res = new Vector2D<T>();
        for (var i = 0; i < 2; ++i)
        {
            res.SetValue(i, array[i]);
        }

        return res;
    }
}