using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;

namespace BlockFactory.Network.Packet_;

[PacketFlags(0)]
public class PlayerPosPacket : IInGamePacket
{
    private ServerControlledPlayerState _state;

    public PlayerPosPacket(ServerControlledPlayerState state)
    {
        _state = state;
    }

    public PlayerPosPacket() : this(default)
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
        BlockFactoryClient.Player.MotionController.SetServerState(_state);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}