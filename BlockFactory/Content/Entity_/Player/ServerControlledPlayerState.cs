using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Content.Entity_.Player;

public struct ServerControlledPlayerState : IBinarySerializable
{
    public Vector3D<double> Pos;
    public Vector3D<double> Velocity;
    public bool IsStandingOnGround;
    public long MotionTick;

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        Pos.SerializeBinary(writer);
        Velocity.SerializeBinary(writer);
        writer.Write(MotionTick);
        writer.Write(IsStandingOnGround);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        Pos.DeserializeBinary(reader);
        Velocity.DeserializeBinary(reader);
        MotionTick = reader.ReadInt64();
        IsStandingOnGround = reader.ReadBoolean();
    }
}