namespace BlockFactory.Serialization;

public class Int16Tag : IValueBasedTag<short>
{
    public Int16Tag(short value)
    {
        Value = value;
    }

    public Int16Tag()
    {
    }

    public TagType Type => TagType.Int16;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(BinaryReader reader)
    {
        Value = reader.ReadInt16();
    }

    public short Value { get; set; }
}