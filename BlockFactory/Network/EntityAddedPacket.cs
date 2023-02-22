using BlockFactory.Entity_;
using BlockFactory.Game;
using BlockFactory.Init;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Network;

public class EntityAddedPacket : IInGamePacket
{
    private EntityType? _type;
    private DictionaryTag? _tag;
    private Entity? _entity;
    public EntityAddedPacket(BinaryReader reader)
    {
        _type = Entities.Registry[reader.ReadInt32()];
        _tag = new DictionaryTag();
        _tag.Read(reader);
        _entity = _type.EntityCreator();
        ((ITagSerializable)_entity).DeserializeFromTag(_tag);
    }

    public EntityAddedPacket(EntityType type, DictionaryTag entityTag)
    {
        _type = type;
        _tag = entityTag;
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write(_type!.Id);
        _tag!.Write(writer);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }

    public void Process(NetworkConnection connection)
    {
        connection.GameInstance!.World.AddEntity(_entity!, true);
    }
}