using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public struct ServerControlledPlayerState : IBinarySerializable
{
    public Vector3D<double> Pos;
    public Vector3D<double> Velocity;
    public long MotionTick;

    public ServerControlledPlayerState(Vector3D<double> pos, Vector3D<double> velocity, long motionTick)
    {
        Pos = pos;
        Velocity = velocity;
        MotionTick = motionTick;
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        Pos.SerializeBinary(writer);
        Velocity.SerializeBinary(writer);
        writer.Write(MotionTick);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        Pos.DeserializeBinary(reader);
        Velocity.DeserializeBinary(reader);
        MotionTick = reader.ReadInt64();
    }
}