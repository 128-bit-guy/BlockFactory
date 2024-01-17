using BlockFactory.Entity_;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(0)]
public class PlayerControlPacket : IPacket
{
    private ClientControlledPlayerState _state;

    public PlayerControlPacket(ClientControlledPlayerState state)
    {
        _state = state;
    }

    public PlayerControlPacket() : this(default)
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _state.SerializeBinary(writer, reason);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _state.DeserializeBinary(reader, reason);
    }

    public void Handle(PlayerEntity? sender)
    {
        sender!.MotionController.ClientState = _state;
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Server;
    }
}