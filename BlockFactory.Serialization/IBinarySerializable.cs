namespace BlockFactory.Serialization;

public interface IBinarySerializable
{
    void SerializeBinary(BinaryWriter writer, SerializationReason reason);
    void DeserializeBinary(BinaryReader reader, SerializationReason reason);
}