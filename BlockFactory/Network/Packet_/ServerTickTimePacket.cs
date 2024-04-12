using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Serialization;

namespace BlockFactory.Network.Packet_;

[PacketFlags(0)]
public class ServerTickTimePacket : IInGamePacket
{
    private float _tickTime;

    public ServerTickTimePacket(float tickTime)
    {
        _tickTime = tickTime;
    }

    public ServerTickTimePacket()
    {
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write(_tickTime);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _tickTime = reader.ReadSingle();
    }

    public void Handle(PlayerEntity? sender)
    {
        BfDebug.HandleTickTime(_tickTime);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}