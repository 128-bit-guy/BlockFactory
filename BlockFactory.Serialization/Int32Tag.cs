namespace BlockFactory.Serialization;

public class Int32Tag : IValueBasedTag<int>
{
    public Int32Tag(int value)
    {
        Value = value;
    }

    public Int32Tag()
    {
    }

    public TagType Type => TagType.Int32;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(BinaryReader reader)
    {
        Value = reader.ReadInt32();
    }

    public int Value { get; set; }
}