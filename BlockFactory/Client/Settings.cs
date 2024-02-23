using BlockFactory.Base;
using BlockFactory.Serialization;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public class Settings : ITagSerializable
{
    public Credentials Credentials;

    public Settings(Credentials credentials)
    {
        Credentials = credentials;
    }

    public Settings() : this(new Credentials())
    {
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.Set("credentials", Credentials.SerializeToTag(reason));
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        Credentials.DeserializeFromTag(tag.Get<DictionaryTag>("credentials"), reason);
    }
}