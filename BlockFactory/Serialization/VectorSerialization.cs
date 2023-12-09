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
}