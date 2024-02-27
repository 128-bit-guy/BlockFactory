namespace BlockFactory.Serialization;

public interface ITagSerializable : IBinarySerializable
{
    void IBinarySerializable.SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        SerializeToTag(reason).Write(writer);
    }

    void IBinarySerializable.DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var tag = new DictionaryTag();
        tag.Read(reader);
        DeserializeFromTag(tag, reason);
    }

    DictionaryTag SerializeToTag(SerializationReason reason);
    void DeserializeFromTag(DictionaryTag tag, SerializationReason reason);
}