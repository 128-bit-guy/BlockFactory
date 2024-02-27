namespace BlockFactory.Serialization;

public class ByteArrayTag : IValueBasedTag<byte[]>
{
    public ByteArrayTag(byte[] value)
    {
        Value = (byte[])value.Clone();
    }

    public ByteArrayTag()
    {
        Value = Array.Empty<byte>();
    }

    public TagType Type => TagType.ByteArray;

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(Value.Length);
        foreach (var b in Value) writer.Write(b);
    }

    public void Read(BinaryReader reader)
    {
        Value = new byte[reader.Read7BitEncodedInt()];
        for (var i = 0; i < Value.Length; ++i) Value[i] = reader.ReadByte();
    }

    public byte[] Value { get; set; }
}