namespace BlockFactory.Serialization;

public class SingleArrayTag : IValueBasedTag<float[]>
{
    public SingleArrayTag(float[] value)
    {
        Value = (float[])value.Clone();
    }

    public SingleArrayTag()
    {
        Value = Array.Empty<float>();
    }

    public TagType Type => TagType.SingleArray;

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(Value.Length);
        foreach (var s in Value) writer.Write(s);
    }

    public void Read(BinaryReader reader)
    {
        Value = new float[reader.Read7BitEncodedInt()];
        for (var i = 0; i < Value.Length; ++i) Value[i] = reader.ReadSingle();
    }

    public float[] Value { get; set; }
}