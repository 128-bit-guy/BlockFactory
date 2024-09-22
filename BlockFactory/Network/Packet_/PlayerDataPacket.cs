using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class PlayerDataPacket : IInGamePacket
{
    private byte[]? _data;

    public PlayerDataPacket()
    {
    }

    public PlayerDataPacket(PlayerEntity player)
    {
        _data = TagIO.Write(player.SerializeToTag(SerializationReason.NetworkInit));
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
        PlayerEntity player = new ClientPlayerEntity();
        player.DeserializeFromTag(TagIO.Read(_data!), SerializationReason.NetworkInit);
        BlockFactoryClient.SetPlayer(player, false);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}