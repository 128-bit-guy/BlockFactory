using BlockFactory.Client;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class PlayerWorldControlPacket : IInGamePacket
{
    private int _id;
    public PlayerWorldControlPacket() : this(0)
    {
        
    }

    public PlayerWorldControlPacket(int id)
    {
        _id = id;
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write(_id);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _id = reader.ReadInt32();
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        if (_id == -1)
        {
            BlockFactoryClient.Player!.World!.RemoveEntityInternal(BlockFactoryClient.Player, false);
        }
        else
        {
            BlockFactoryClient.LogicProcessor!.GetWorld().AddEntityInternal(BlockFactoryClient.Player!, false);
        }
    }
}