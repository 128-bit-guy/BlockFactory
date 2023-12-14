using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class BlockChangePacket : IPacket
{
    private Vector3D<int> _pos;
    private short _block;

    public BlockChangePacket(Vector3D<int> pos, short block)
    {
        _pos = pos;
        _block = block;
    }

    public BlockChangePacket() : this(Vector3D<int>.Zero, 0)
    {
        
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _pos.SerializeBinary(writer);
        writer.Write(_block);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _pos.DeserializeBinary(reader);
        _block = reader.ReadInt16();
    }

    public void Handle(PlayerEntity? sender)
    {
        BlockFactoryClient.Player.World!.SetBlock(_pos, _block);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}