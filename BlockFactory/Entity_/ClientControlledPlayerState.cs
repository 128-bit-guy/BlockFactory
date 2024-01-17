using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public struct ClientControlledPlayerState : IBinarySerializable
{
    public Vector2D<float> HeadRotation;
    public PlayerControlState ControlState;
    public long MotionTick;

    public ClientControlledPlayerState(Vector2D<float> headRotation, PlayerControlState controlState, long motionTick)
    {
        HeadRotation = headRotation;
        ControlState = controlState;
        MotionTick = motionTick;
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        HeadRotation.SerializeBinary(writer);
        writer.Write7BitEncodedInt((int)ControlState);
        writer.Write(MotionTick);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        HeadRotation.DeserializeBinary(reader);
        ControlState = (PlayerControlState)reader.Read7BitEncodedInt();
        MotionTick = reader.ReadInt64();
    }
}