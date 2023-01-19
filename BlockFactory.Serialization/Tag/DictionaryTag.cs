namespace BlockFactory.Serialization.Tag;

public class DictionaryTag : ITag
{
    private readonly Dictionary<string, ITag> _dictionary = new();
    public TagType Type => TagType.Dictionary;

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(_dictionary.Count);
        foreach (var (key, tag) in _dictionary)
        {
            writer.Write(key);
            writer.Write((byte)tag.Type);
            tag.Write(writer);
        }
    }

    public void Read(BinaryReader reader)
    {
        _dictionary.Clear();
        var cnt = reader.Read7BitEncodedInt();
        for (var i = 0; i < cnt; ++i)
        {
            var key = reader.ReadString();
            var type = (TagType)reader.ReadByte();
            var tag = TagTypes.CreateTag(type);
            tag.Read(reader);
            _dictionary.Add(key, tag);
        }
    }

    public T Get<T>(string s) where T : ITag, new()
    {
        if (_dictionary.TryGetValue(s, out var value))
        {
            if (value is T t)
                return t;
            return new T();
        }

        return new T();
    }

    public T GetValue<T>(string s)
    {
        var t = TagTypes.GetValueBasedTagType<T>();
        if (_dictionary.TryGetValue(s, out var value) && value.Type == t)
            return TagTypes.GetValue((IValueBasedTag<T>)value);

        return TagTypes.CreateDefaultValueBasedTag<T>().Value;
    }

    public void Set(string s, ITag tag)
    {
        _dictionary[s] = tag;
    }

    public void SetValue<T>(string s, T value)
    {
        _dictionary[s] = TagTypes.CreateValueBasedTag(value);
    }
}