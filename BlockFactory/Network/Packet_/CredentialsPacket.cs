using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class CredentialsPacket : IPacket
{
    public readonly Credentials Credentials;

    public CredentialsPacket(Credentials credentials)
    {
        Credentials = credentials;
    }

    public CredentialsPacket() : this(new Credentials())
    {
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        Credentials.SerializeToTag(SerializationReason.NetworkInit).Write(writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var tag = new DictionaryTag();
        tag.Read(reader);
        Credentials.DeserializeFromTag(tag, SerializationReason.NetworkInit);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Server;
    }
}