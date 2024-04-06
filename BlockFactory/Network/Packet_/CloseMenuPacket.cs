using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class CloseMenuPacket : IInGamePacket
{
    public CloseMenuPacket()
    {
        
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}