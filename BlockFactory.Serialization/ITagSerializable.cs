namespace BlockFactory.Serialization;

public interface ITagSerializable : IBinarySerializable
{
    DictionaryTag SerializeToTag(SerializationReason reason);
    void DeserializeFromTag(DictionaryTag tag, SerializationReason reason);

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
}