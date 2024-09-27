using BlockFactory.Client;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using ENet.Managed;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class EntityChangeChunkPacket : IInGamePacket
{
    private Guid _guid;
    private Vector3D<int> _from;
    private Vector3D<int> _to;

    public EntityChangeChunkPacket()
    {
        
    }

    public EntityChangeChunkPacket(Guid guid, Vector3D<int> from, Vector3D<int> to)
    {
        _guid = guid;
        _from = from;
        _to = to;
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        _guid.TryWriteBytes(guidBytes);
        writer.Write(guidBytes);
        _from.SerializeBinary(writer);
        _to.SerializeBinary(writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        for (var i = 0; i < 16; ++i)
        {
            guidBytes[i] = reader.ReadByte();
        }
        _guid = new Guid(guidBytes);
        _from.DeserializeBinary(reader);
        _to.DeserializeBinary(reader);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        var fromChunk = BlockFactoryClient.Player!.World!.GetChunk(_from)!;
        var toChunk = BlockFactoryClient.Player.World!.GetChunk(_to)!;
        var entity = fromChunk.GetEntity(_guid)!;
        fromChunk.MoveEntityToChunk(entity, toChunk);
    }
}