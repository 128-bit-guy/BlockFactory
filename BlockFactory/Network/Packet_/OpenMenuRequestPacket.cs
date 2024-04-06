using BlockFactory.Entity_;
using BlockFactory.Gui.Menu_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class OpenMenuRequestPacket : IInGamePacket
{
    private OpenMenuRequestType _requestType;

    public OpenMenuRequestPacket(OpenMenuRequestType requestType)
    {
        _requestType = requestType;
    }

    public OpenMenuRequestPacket() : this(OpenMenuRequestType.Message)
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write((byte)_requestType);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _requestType = (OpenMenuRequestType)reader.ReadByte();
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Server;
    }

    public void Handle(PlayerEntity? sender)
    {
        sender.HandleOpenMenuRequest(_requestType);
    }
}