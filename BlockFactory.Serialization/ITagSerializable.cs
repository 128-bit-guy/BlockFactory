namespace BlockFactory.Serialization;

public interface ITagSerializable
{
    DictionaryTag SerializeToTag();
    void DeserializeFromTag(DictionaryTag tag);
}