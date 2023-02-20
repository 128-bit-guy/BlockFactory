using BlockFactory.Game;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Network;

public class CredentialsPacket : IPacket
{
    public readonly Credentials Credentials;

    public CredentialsPacket(Credentials credentials)
    {
        Credentials = credentials;
    }

    public CredentialsPacket(BinaryReader reader) : this(new Credentials())
    {
        var tag = new DictionaryTag();
        tag.Read(reader);
        ((ITagSerializable)Credentials).DeserializeFromTag(tag);
    }

    public void Write(BinaryWriter writer)
    {
        var tag = ((ITagSerializable)Credentials).SerializeToTag();
        tag.Write(writer);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerBackend;
    }
}