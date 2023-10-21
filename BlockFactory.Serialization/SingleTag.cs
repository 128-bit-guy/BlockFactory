namespace BlockFactory.Serialization;

public class SingleTag : IValueBasedTag<float>
{
    public SingleTag(float value)
    {
        Value = value;
    }

    public SingleTag()
    {
    }

    public TagType Type => TagType.Single;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(BinaryReader reader)
    {
        Value = reader.ReadSingle();
    }

    public float Value { get; set; }
}