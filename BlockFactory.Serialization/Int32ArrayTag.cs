namespace BlockFactory.Serialization;

public class Int32ArrayTag : IValueBasedTag<int[]>
{
    public Int32ArrayTag(int[] value)
    {
        Value = (int[])value.Clone();
    }

    public Int32ArrayTag()
    {
        Value = Array.Empty<int>();
    }

    public TagType Type => TagType.Int32Array;

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(Value.Length);
        foreach (var s in Value) writer.Write(s);
    }

    public void Read(BinaryReader reader)
    {
        Value = new int[reader.Read7BitEncodedInt()];
        for (var i = 0; i < Value.Length; ++i) Value[i] = reader.ReadInt32();
    }

    public int[] Value { get; set; }
}