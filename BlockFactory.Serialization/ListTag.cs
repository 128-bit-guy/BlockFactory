namespace BlockFactory.Serialization;

public class ListTag : ITag
{
    private ITag[] _elements;

    public ListTag()
    {
    }

    public ListTag(ITag[] elements, TagType elementType)
    {
        _elements = elements;
        ElementType = elementType;
    }

    public ListTag(int cnt, TagType elementType) : this(new ITag[cnt], elementType)
    {
    }

    public TagType ElementType { get; private set; }
    public TagType Type => TagType.List;

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(_elements.Length);
        writer.Write((byte)ElementType);
        foreach (var tag in _elements) tag.Write(writer);
    }

    public void Read(BinaryReader reader)
    {
        var cnt = reader.Read7BitEncodedInt();
        _elements = new ITag[cnt];
        ElementType = (TagType)reader.ReadByte();
        for (var i = 0; i < cnt; ++i)
        {
            var tag = TagTypes.CreateTag(ElementType);
            tag.Read(reader);
            _elements[i] = tag;
        }
    }

    public T Get<T>(int i) where T : ITag
    {
        var value = _elements[i];
        if (value is T t)
            return t;
        throw new InvalidOperationException("Tried to set element of list of invalid type");
    }

    public T GetValue<T>(int i)
    {
        var t = TagTypes.GetValueBasedTagType<T>();
        var value = _elements[i];
        if (value.Type == t)
            return TagTypes.GetValue((IValueBasedTag<T>)value);
        throw new InvalidOperationException("Tried to set element of list of invalid type");
    }

    public void Set(int i, ITag tag)
    {
        if (tag.Type == ElementType)
            _elements[i] = tag;
        else
            throw new InvalidOperationException("Tried to set element of list of invalid type");
    }

    public void SetValue<T>(int i, T value)
    {
        if (TagTypes.GetValueBasedTagType<T>() == ElementType)
            _elements[i] = TagTypes.CreateValueBasedTag(value);
        else
            throw new InvalidOperationException("Tried to set element of list of invalid type");
    }

    public IEnumerable<T> GetEnumerable<T>() where T : ITag
    {
        for (var i = 0; i < _elements.Length; ++i) yield return Get<T>(i);
    }

    public IEnumerable<T> GetValueEnumerable<T>()
    {
        for (var i = 0; i < _elements.Length; ++i) yield return GetValue<T>(i);
    }
}