namespace BlockFactory.Serialization.Serializable;

public interface IBinarySerializable
{
    void SerializeToBinaryWriter(BinaryWriter writer);
    void DeserializeFromBinaryReader(BinaryReader reader);
}