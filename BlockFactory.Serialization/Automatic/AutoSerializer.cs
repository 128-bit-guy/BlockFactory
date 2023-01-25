using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic;

public class AutoSerializer
{
    private readonly Action<object, BinaryReader> _fromBinaryReaderDeserializer;
    private readonly Action<object, DictionaryTag> _fromTagDeserializer;
    private readonly Action<object, BinaryWriter> _toBinaryWriterSerializer;
    private readonly Func<object, DictionaryTag> _toTagSerializer;

    internal AutoSerializer(Func<object, DictionaryTag> toTagSerializer,
        Action<object, DictionaryTag> fromTagDeserializer, Action<object, BinaryWriter> toBinaryWriterSerializer,
        Action<object, BinaryReader> fromBinaryReaderDeserializer)
    {
        _toTagSerializer = toTagSerializer;
        _fromTagDeserializer = fromTagDeserializer;
        _toBinaryWriterSerializer = toBinaryWriterSerializer;
        _fromBinaryReaderDeserializer = fromBinaryReaderDeserializer;
    }

    public DictionaryTag SerializeToTag(object obj)
    {
        return _toTagSerializer(obj);
    }

    public void DeserializeFromTag(object obj, DictionaryTag tag)
    {
        _fromTagDeserializer(obj, tag);
    }

    public void SerializeToBinaryWriter(object obj, BinaryWriter writer)
    {
        _toBinaryWriterSerializer(obj, writer);
    }

    public void DeserializeFromBinaryReader(object obj, BinaryReader reader)
    {
        _fromBinaryReaderDeserializer(obj, reader);
    }
}