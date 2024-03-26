using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class PlayerUpdatePacket : IInGamePacket
{
    private byte[]? _data;

    public PlayerUpdatePacket()
    {
    }

    public PlayerUpdatePacket(PlayerEntity player)
    {
        _data = TagIO.Write(player.SerializeToTag(SerializationReason.NetworkUpdate));
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write7BitEncodedInt(_data!.Length);
        writer.Write(_data!);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var length = reader.Read7BitEncodedInt();
        _data = reader.ReadBytes(length);
    }

    public void Handle(PlayerEntity? sender)
    {
        BlockFactoryClient.Player!.DeserializeFromTag(TagIO.Read(_data!), SerializationReason.NetworkUpdate);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}