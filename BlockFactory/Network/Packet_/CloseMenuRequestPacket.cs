using BlockFactory.Content.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class CloseMenuRequestPacket : IInGamePacket
{
    public CloseMenuRequestPacket()
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
        return side == LogicalSide.Server;
    }

    public void Handle(PlayerEntity? sender)
    {
        if (!sender!.MenuManager.Empty)
        {
            sender.MenuManager.Top!.EscapePressed();
        }
    }
}