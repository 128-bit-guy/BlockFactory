using BlockFactory.Client;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class WorldTimeUpdatePacket : IInGamePacket
{
    private long _worldTime;

    public WorldTimeUpdatePacket(long worldTime)
    {
        _worldTime = worldTime;
    }

    public WorldTimeUpdatePacket()
    {
        
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write(_worldTime);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _worldTime = reader.ReadInt64();
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        BlockFactoryClient.Player!.World!.WorldTimeManager.WorldTime = _worldTime;
    }
}