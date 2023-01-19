namespace BlockFactory.Serialization.Tag;

public interface ITag
{
    TagType Type { get; }
    void Write(BinaryWriter writer);
    void Read(BinaryReader reader);
}