using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class ChunkUnloadPacket : IInGamePacket
{
    private Vector3D<int> _pos;

    public ChunkUnloadPacket(Vector3D<int> pos)
    {
        _pos = pos;
    }

    public ChunkUnloadPacket() : this(Vector3D<int>.Zero)
    {
    }


    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _pos.SerializeBinary(writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _pos.DeserializeBinary(reader);
    }

    public void Handle(PlayerEntity? sender)
    {
        var player = BlockFactoryClient.Player;
        var c = player.World!.GetChunk(_pos, false);
        if (c == null) return;
        player.World!.RemoveChunk(_pos);
        player.ChunkLoader!.RemoveVisibleChunk(c);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}