using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Serializable;

public interface ITagSerializable
{
    DictionaryTag SerializeToTag();
    void DeserializeFromTag(DictionaryTag tag);
}