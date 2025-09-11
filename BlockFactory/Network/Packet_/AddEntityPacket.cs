using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using BlockFactory.World_;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class AddEntityPacket : IInGamePacket
{
    private Guid _guid;
    private int _type;
    private DictionaryTag? _tag;
    private Vector3D<int> _chunkPos;
    public AddEntityPacket()
    {
        
    }

    public AddEntityPacket(Vector3D<int> chunkPos, Entity entity)
    {
        _guid = entity.Guid;
        _type = entity.Type.Id;
        _tag = entity.SerializeToTag(SerializationReason.NetworkInit);
        _chunkPos = chunkPos;
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        _guid.TryWriteBytes(guidBytes);
        writer.Write(guidBytes);
        writer.Write(_type);
        _chunkPos.SerializeBinary(writer);
        TagIO.Write(_tag!, writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        for (var i = 0; i < 16; ++i)
        {
            guidBytes[i] = reader.ReadByte();
        }
        _guid = new Guid(guidBytes);
        _type = reader.ReadInt32();
        _chunkPos.DeserializeBinary(reader);
        _tag = TagIO.Read<DictionaryTag>(reader);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        if (_guid == BlockFactoryClient.Player!.Guid)
        {
            var c = BlockFactoryClient.Player.World!.GetChunk(_chunkPos)!;
            c.Data!.AddEntity(BlockFactoryClient.Player);
            c.AddEntityInternal(BlockFactoryClient.Player, false);
        }
        else
        {
            var entity = Entities.Registry[_type]!.Creator();
            entity.DeserializeFromTag(_tag!, SerializationReason.NetworkInit);
            BlockFactoryClient.Player!.World!.GetChunk(_chunkPos)!.AddEntity(entity);
        }
    }
}