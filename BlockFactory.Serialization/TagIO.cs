using ZstdSharp;

namespace BlockFactory.Serialization;

public static class TagIO
{
    public static T Read<T>(BinaryReader reader) where T : ITag, new()
    {
        var type = (TagType)reader.ReadByte();
        var tag = TagTypes.CreateTag(type);
        tag.Read(reader);
        if (tag is T t)
            return t;
        return new T();
    }

    public static void Write(ITag t, BinaryWriter writer)
    {
        writer.Write((byte)t.Type);
        t.Write(writer);
    }

    public static DictionaryTag? Read(string file)
    {
        if (!File.Exists(file)) return null;
        var b = File.ReadAllBytes(file);
        if (BitConverter.IsLittleEndian) Array.Reverse(b, 0, sizeof(int));

        var uncompressedSize = BitConverter.ToInt32(b);
        var uncompressed = Zstd.Decompress(b, sizeof(int), b.Length - sizeof(int), uncompressedSize);
        using var stream = new MemoryStream(uncompressed);
        using var reader = new BinaryReader(stream);
        var tag = new DictionaryTag();
        tag.Read(reader);
        return tag;
    }

    public static void Write(string file, DictionaryTag tag)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        tag.Write(writer);
        var uncompressed = stream.ToArray();
        var compressed = Zstd.Compress(uncompressed);
        var res = new byte[compressed.Length + sizeof(int)];
        Array.Copy(compressed, 0, res, sizeof(int), compressed.Length);
        BitConverter.TryWriteBytes(res, uncompressed.Length);
        if (BitConverter.IsLittleEndian) Array.Reverse(res, 0, sizeof(int));
        File.WriteAllBytes(file, res);
    }

    public static void Deserialize(string file, ITagSerializable s)
    {
        var tag = Read(file);
        if (tag != null)
        {
            s.DeserializeFromTag(tag, SerializationReason.Save);
        }
    }

    public static void Serialize(string file, ITagSerializable s)
    {
        var tag = s.SerializeToTag(SerializationReason.Save);
        Write(file, tag);
    }

    // public static T ReadCompressed<T>(ReadOnlySpan<byte> b) where T : ITag, new()
    // {
    //     using var decompressor = new Decompressor();
    //     var s = decompressor.Unwrap(b);
    //     var decompressed = s.ToArray();
    //     using var stream = new MemoryStream(decompressed);
    //     using var reader = new BinaryReader(stream);
    //     return Read<T>(reader);
    // }
    //
    // public static byte[] WriteCompressed(ITag tag)
    // {
    //     using var stream = new MemoryStream();
    //     using var writer = new BinaryWriter(stream);
    //     Write(tag, writer);
    //     var b = stream.ToArray();
    //     using var compressor = new Compressor();
    //     var s = compressor.Wrap(b);
    //     var decompressed = s.ToArray();
    //     return decompressed;
    // }
}