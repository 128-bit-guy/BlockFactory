namespace BlockFactory.Serialization.Tag;

public class Int64Tag : IValueBasedTag<long>
{
    public Int64Tag(long value)
    {
        Value = value;
    }

    public Int64Tag()
    {
    }

    public TagType Type => TagType.Int64;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(BinaryReader reader)
    {
        Value = reader.ReadInt64();
    }

    public long Value { get; set; }
}