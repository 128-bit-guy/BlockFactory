using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using BlockFactory.World_.Light;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class LightChangePacket : IPacket
{
    private Vector3D<int> _pos;
    private LightChannel _channel;
    private byte _light;

    public LightChangePacket(Vector3D<int> pos, LightChannel channel, byte light)
    {
        _pos = pos;
        _channel = channel;
        _light = light;
    }

    public LightChangePacket() : this(Vector3D<int>.Zero, LightChannel.Block, 0)
    {
    }


    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _pos.SerializeBinary(writer);
        writer.Write((byte)_channel);
        writer.Write(_light);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _pos.DeserializeBinary(reader);
        _channel = (LightChannel)reader.ReadByte();
        _light = reader.ReadByte();
    }

    public void Handle(PlayerEntity? sender)
    {
        BlockFactoryClient.Player.World!.SetLight(_pos, _channel, _light);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}