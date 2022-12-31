namespace BlockFactory.Serialization;

public interface ITag
{
    TagType Type { get; }
    void Write(BinaryWriter writer);
    void Read(BinaryReader reader);
}