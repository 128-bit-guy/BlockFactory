using BlockFactory.Client;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(0)]
public class EntityPosUpdatePacket : IInGamePacket
{
    private Vector3D<int> _chunkPos;
    private List<(Guid, Vector3D<double>)>? _posChanges;

    public EntityPosUpdatePacket()
    {
        
    }

    public EntityPosUpdatePacket(Vector3D<int> chunkPos, List<(Guid, Vector3D<double>)> posChanges)
    {
        _chunkPos = chunkPos;
        _posChanges = posChanges;
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _chunkPos.SerializeBinary(writer);
        writer.Write7BitEncodedInt(_posChanges!.Count);
        Span<byte> guidBytes = stackalloc byte[16];
        foreach (var (guid, newPos) in _posChanges)
        {
            guid.TryWriteBytes(guidBytes);
            writer.Write(guidBytes);
            newPos.SerializeBinary(writer);
        }
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _chunkPos.DeserializeBinary(reader);
        var cnt = reader.Read7BitEncodedInt();
        _posChanges = new List<(Guid, Vector3D<double>)>(cnt);
        Span<byte> guidBytes = stackalloc byte[16];
        for (var j = 0; j < cnt; ++j)
        {
            for (var i = 0; i < 16; ++i)
            {
                guidBytes[i] = reader.ReadByte();
            }
            var guid = new Guid(guidBytes);
            Vector3D<double> newPos = default;
            newPos.DeserializeBinary(reader);
            _posChanges.Add((guid, newPos));
        }
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        var c = BlockFactoryClient.Player!.World!.GetChunk(_chunkPos)!;
        foreach (var (guid, newPos) in _posChanges!)
        {
            c.GetEntity(guid)!.SetPos(newPos);
        }
    }
}