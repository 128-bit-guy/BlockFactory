using BlockFactory.Entity_;
using BlockFactory.Registry_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class RegistryMappingPacket : IPacket
{
    private readonly RegistryMapping _mapping;

    public RegistryMappingPacket(RegistryMapping mapping)
    {
        _mapping = mapping;
    }

    public RegistryMappingPacket() : this(new RegistryMapping())
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _mapping.SerializeToTag(SerializationReason.NetworkInit).Write(writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var tag = new DictionaryTag();
        tag.Read(reader);
        _mapping.DeserializeFromTag(tag, SerializationReason.NetworkInit);
    }

    public void Handle(PlayerEntity? sender)
    {
        SynchronizedRegistries.LoadMapping(_mapping);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}