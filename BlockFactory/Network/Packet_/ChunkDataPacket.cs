using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using BlockFactory.World_;
using BlockFactory.World_.Serialization;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
[CompressedPacket]
public class ChunkDataPacket : IInGamePacket
{
    private readonly ChunkData _data;
    private Vector3D<int> _pos;
    private DictionaryTag _tagData;

    public ChunkDataPacket(Vector3D<int> pos, ChunkData data)
    {
        _data = data;
        _pos = pos;
        _tagData = data.WriteTagData(SerializationReason.NetworkInit);
    }

    public ChunkDataPacket() : this(Vector3D<int>.Zero, new ChunkData())
    {
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _pos.SerializeBinary(writer);
        _data.SerializeBinary(writer, SerializationReason.NetworkInit);
        TagIO.Write(_tagData, writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _pos.DeserializeBinary(reader);
        _data.DeserializeBinary(reader, SerializationReason.NetworkInit);
        _tagData = TagIO.Read<DictionaryTag>(reader);
        _data.ReadTagData(_tagData, SerializationReason.NetworkInit);
    }

    public void Handle(PlayerEntity? sender)
    {
        var player = BlockFactoryClient.Player;
        var c = new Chunk(player.World!, _pos, null)
        {
            Data = _data
        };
        player.World!.AddChunk(c);
        c.OnLoaded(false);
        player.ChunkLoader!.AddVisibleChunk(c);
        player.World.ChunkStatusManager.ScheduleStatusUpdate(c);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}