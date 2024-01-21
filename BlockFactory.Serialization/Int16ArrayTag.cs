namespace BlockFactory.Serialization;

public class Int16ArrayTag : IValueBasedTag<short[]>
{
    public Int16ArrayTag(short[] value)
    {
        Value = (short[])value.Clone();
    }

    public Int16ArrayTag()
    {
        Value = Array.Empty<short>();
    }

    public TagType Type => TagType.Int16Array;

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(Value.Length);
        foreach (var s in Value)
        {
            writer.Write(s);
        }
    }

    public void Read(BinaryReader reader)
    {
        Value = new short[reader.Read7BitEncodedInt()];
        for (var i = 0; i < Value.Length; ++i)
        {
            Value[i] = reader.ReadInt16();
        }
    }

    public short[] Value { get; set; }
}