using BlockFactory.Util.Math_;

namespace BlockFactory.Network;

public class EntityPosUpdatePacket : IPacket
{
    public readonly long Id;
    public readonly EntityPos Pos;

    public EntityPosUpdatePacket(BinaryReader reader)
    {
        Pos = new EntityPos(reader);
        Id = reader.ReadInt64();
    }

    public EntityPosUpdatePacket(EntityPos pos, long id)
    {
        Pos = pos;
        Id = id;
    }

    public void Write(BinaryWriter writer)
    {
        Pos.Write(writer);
        writer.Write(Id);
    }
}