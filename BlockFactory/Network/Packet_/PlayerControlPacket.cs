using BlockFactory.Entity_;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(0)]
public class PlayerControlPacket : IPacket
{
    private Vector2D<float> _headRotation;
    private PlayerControlState _controlState;

    public PlayerControlPacket(Vector2D<float> headRotation, PlayerControlState controlState)
    {
        _headRotation = headRotation;
        _controlState = controlState;
    }

    public PlayerControlPacket() : this(Vector2D<float>.Zero, 0)
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _headRotation.SerializeBinary(writer);
        writer.Write7BitEncodedInt((int)_controlState);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _headRotation.DeserializeBinary(reader);
        _controlState = (PlayerControlState)reader.Read7BitEncodedInt();
    }

    public void Handle(PlayerEntity? sender)
    {
        sender!.HeadRotation = _headRotation;
        sender.ControlState = _controlState;
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Server;
    }
}