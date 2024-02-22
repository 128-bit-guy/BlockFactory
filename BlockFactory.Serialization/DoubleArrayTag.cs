namespace BlockFactory.Serialization;

public class DoubleArrayTag : IValueBasedTag<double[]>
{
    public DoubleArrayTag(double[] value)
    {
        Value = (double[])value.Clone();
    }

    public DoubleArrayTag()
    {
        Value = Array.Empty<double>();
    }

    public TagType Type => TagType.DoubleArray;

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
        Value = new double[reader.Read7BitEncodedInt()];
        for (var i = 0; i < Value.Length; ++i)
        {
            Value[i] = reader.ReadDouble();
        }
    }

    public double[] Value { get; set; }
}