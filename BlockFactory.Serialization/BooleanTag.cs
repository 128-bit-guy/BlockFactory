namespace BlockFactory.Serialization;

public class BooleanTag : IValueBasedTag<bool>
{
    public BooleanTag(bool value)
    {
        Value = value;
    }

    public BooleanTag()
    {
    }

    public TagType Type => TagType.Boolean;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(BinaryReader reader)
    {
        Value = reader.ReadBoolean();
    }

    public bool Value { get; set; }
}