namespace BlockFactory.Serialization.Tag;

public class StringTag : IValueBasedTag<string>
{
    public StringTag(string value)
    {
        Value = value;
    }

    public StringTag() : this(string.Empty)
    {
    }

    public TagType Type => TagType.String;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(BinaryReader reader)
    {
        Value = reader.ReadString();
    }

    public string Value { get; set; }
}