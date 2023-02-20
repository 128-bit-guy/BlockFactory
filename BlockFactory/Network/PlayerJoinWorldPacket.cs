using BlockFactory.Client.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Network;

public class PlayerJoinWorldPacket : IPacket
{
    public readonly PlayerEntity Player;

    public PlayerJoinWorldPacket(BinaryReader reader)
    {
        var tag = new DictionaryTag();
        tag.Read(reader);
        Player = new ClientPlayerEntity(new PlayerInfo(new Credentials()));
        ((ITagSerializable)Player).DeserializeFromTag(tag);
    }

    public PlayerJoinWorldPacket(PlayerEntity entity)
    {
        Player = entity;
    }

    public void Write(BinaryWriter writer)
    {
        var tag = ((ITagSerializable)Player).SerializeToTag();
        tag.Write(writer);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}