using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class RemoveEntityPacket : IInGamePacket
{
    private Guid _guid;
    private Vector3D<int> _chunkPos;

    public RemoveEntityPacket()
    {
        
    }

    public RemoveEntityPacket(Vector3D<int> chunkPos, Guid guid)
    {
        _guid = guid;
        _chunkPos = chunkPos;
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        _guid.TryWriteBytes(guidBytes);
        writer.Write(guidBytes);
        _chunkPos.SerializeBinary(writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        for (var i = 0; i < 16; ++i)
        {
            guidBytes[i] = reader.ReadByte();
        }
        _guid = new Guid(guidBytes);
        _chunkPos.DeserializeBinary(reader);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        var c = BlockFactoryClient.Player!.World!.GetChunk(_chunkPos)!;
        var entity = c.GetEntity(_guid)!;
        if (_guid == BlockFactoryClient.Player.Guid)
        {
            c.RemoveEntityInternal(entity, false);
            c.Data!.RemoveEntity(entity);
        }
        else
        {
            c.RemoveEntity(entity);
        }
    }
}