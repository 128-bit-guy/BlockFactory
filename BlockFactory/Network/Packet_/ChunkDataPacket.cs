using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using BlockFactory.World_;
using BlockFactory.World_.Serialization;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
[CompressedPacket]
public class ChunkDataPacket : IInGamePacket
{
    private Vector3D<int> _pos;
    private ChunkData _data;

    public ChunkDataPacket(Vector3D<int> pos, ChunkData data)
    {
        _data = data;
        _pos = pos;
    }

    public ChunkDataPacket() : this(Vector3D<int>.Zero, new ChunkData())
    {
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _pos.SerializeBinary(writer);
        _data.SerializeBinary(writer, SerializationReason.NetworkInit);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _pos.DeserializeBinary(reader);
        _data.DeserializeBinary(reader, SerializationReason.NetworkInit);
    }

    public void Handle(PlayerEntity? sender)
    {
        var player = BlockFactoryClient.Player;
        var c = new Chunk(player.World!, _pos, null)
        {
            Data = _data
        };
        player.World!.AddChunk(c);
        player.ChunkLoader!.AddVisibleChunk(c);
        player.World.ChunkStatusManager.ScheduleStatusUpdate(c);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}