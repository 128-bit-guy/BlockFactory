using BlockFactory.Serialization;

namespace BlockFactory;

public class Credentials : ITagSerializable
{
    public string Name;
    public string Password;

    public Credentials(string name, string password)
    {
        Name = name;
        Password = password;
    }

    public Credentials() : this(string.Empty, string.Empty)
    {
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.SetValue("name", Name);
        res.SetValue("password", Password);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        Name = tag.GetValue<string>("name");
        Password = tag.GetValue<string>("password");
    }
}