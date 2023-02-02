namespace BlockFactory.Serialization.Tag;

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