namespace BlockFactory.Serialization.Tag;

public class BooleanTag : IValueBasedTag<bool>
{
    public TagType Type => TagType.Boolean;

    public BooleanTag(bool value)
    {
        Value = value;
    }

    public BooleanTag()
    {
        
    }
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