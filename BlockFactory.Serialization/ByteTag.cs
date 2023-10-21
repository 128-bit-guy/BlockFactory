namespace BlockFactory.Serialization;

public class ByteTag : IValueBasedTag<byte>
{
    public ByteTag(byte value)
    {
        Value = value;
    }

    public ByteTag()
    {
    }

    public TagType Type => TagType.Byte;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(BinaryReader reader)
    {
        Value = reader.ReadByte();
    }

    public byte Value { get; set; }
}