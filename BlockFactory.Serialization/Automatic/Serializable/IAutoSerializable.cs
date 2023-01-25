using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic.Serializable;

public interface IAutoSerializable : ITagSerializable, IBinarySerializable
{
    public AutoSerializer AutoSerializer { get; }

    void IBinarySerializable.SerializeToBinaryWriter(BinaryWriter writer)
    {
        AutoSerializer.SerializeToBinaryWriter(this, writer);
        SerializeToBinaryWriterAdditional(writer);
    }

    void IBinarySerializable.DeserializeFromBinaryReader(BinaryReader reader)
    {
        AutoSerializer.DeserializeFromBinaryReader(this, reader);
        DeserializeFromBinaryReaderAdditional(reader);
    }

    DictionaryTag ITagSerializable.SerializeToTag()
    {
        var tag = AutoSerializer.SerializeToTag(this);
        SerializeToTagAdditional(tag);
        return tag;
    }

    void ITagSerializable.DeserializeFromTag(DictionaryTag tag)
    {
        AutoSerializer.DeserializeFromTag(this, tag);
        DeserializeFromTagAdditional(tag);
    }

    void SerializeToTagAdditional(DictionaryTag tag)
    {
    }

    void DeserializeFromTagAdditional(DictionaryTag tag)
    {
    }

    void SerializeToBinaryWriterAdditional(BinaryWriter writer)
    {
    }

    void DeserializeFromBinaryReaderAdditional(BinaryReader reader)
    {
    }
}