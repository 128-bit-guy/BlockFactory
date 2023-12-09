using BlockFactory.CubeMath;
using Silk.NET.Maths;

namespace BlockFactory.Serialization;

public static class VectorSerialization
{
    public static void SerializeBinary(this Vector3D<int> v, BinaryWriter w)
    {
        for (int i = 0; i < 3; ++i)
        {
            w.Write(v[i]);
        }
    }

    public static void DeserializeBinary(this ref Vector3D<int> v, BinaryReader reader)
    {
        for (int i = 0; i < 3; ++i)
        {
            v.SetValue(i, reader.ReadInt32());
        }
    }
    public static void SerializeBinary(this Vector3D<double> v, BinaryWriter w)
    {
        for (int i = 0; i < 3; ++i)
        {
            w.Write(v[i]);
        }
    }

    public static void DeserializeBinary(this ref Vector3D<double> v, BinaryReader reader)
    {
        for (int i = 0; i < 3; ++i)
        {
            v.SetValue(i, reader.ReadDouble());
        }
    }
    public static void SerializeBinary(this Vector2D<float> v, BinaryWriter w)
    {
        for (int i = 0; i < 2; ++i)
        {
            w.Write(v[i]);
        }
    }

    public static void DeserializeBinary(this ref Vector2D<float> v, BinaryReader reader)
    {
        for (int i = 0; i < 2; ++i)
        {
            v.SetValue(i, reader.ReadSingle());
        }
    }
}