using BlockFactory.Content.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class PlayerActionPacket : IInGamePacket
{
    public PlayerAction Action;
    public int Delta;

    public PlayerActionPacket(PlayerAction action, int delta)
    {
        Action = action;
        Delta = delta;
    }

    public PlayerActionPacket() : this(PlayerAction.HotBarAdd, 0)
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write7BitEncodedInt((int)Action);
        writer.Write7BitEncodedInt(Delta);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        Action = (PlayerAction)reader.Read7BitEncodedInt();
        Delta = reader.Read7BitEncodedInt();
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Server;
    }

    public void Handle(PlayerEntity? sender)
    {
        sender!.ProcessPlayerAction(Action, Delta);
    }
}