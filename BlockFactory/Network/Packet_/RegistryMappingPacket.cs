using BlockFactory.Registry_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class RegistryMappingPacket : IPacket
{
    public readonly RegistryMapping Mapping;

    public RegistryMappingPacket(RegistryMapping mapping)
    {
        Mapping = mapping;
    }

    public RegistryMappingPacket() : this(new RegistryMapping())
    {
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        Mapping.SerializeToTag(SerializationReason.NetworkInit).Write(writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var tag = new DictionaryTag();
        tag.Read(reader);
        Mapping.DeserializeFromTag(tag, SerializationReason.NetworkInit);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}