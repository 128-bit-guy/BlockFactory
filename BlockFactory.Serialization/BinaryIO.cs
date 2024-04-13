using System.Net;
using ZstdSharp;

namespace BlockFactory.Serialization;

public static class BinaryIO
{
    public static void Deserialize(string file, IBinarySerializable serializable)
    {
        if (!File.Exists(file)) return;
        var b = File.ReadAllBytes(file);
        if (BitConverter.IsLittleEndian) Array.Reverse(b, 0, sizeof(int));

        var uncompressedSize = BitConverter.ToInt32(b);
        var uncompressed = Zstd.Decompress(b, sizeof(int), b.Length - sizeof(int), uncompressedSize);
        using var stream = new MemoryStream(uncompressed);
        using var reader = new BinaryReader(stream);
        serializable.DeserializeBinary(reader, SerializationReason.Save);
    }

    public static void Serialize(string file, IBinarySerializable serializable)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        serializable.SerializeBinary(writer, SerializationReason.Save);
        var uncompressed = stream.ToArray();
        var compressed = Zstd.Compress(uncompressed);
        var res = new byte[compressed.Length + sizeof(int)];
        Array.Copy(compressed, 0, res, sizeof(int), compressed.Length);
        BitConverter.TryWriteBytes(res, uncompressed.Length);
        if (BitConverter.IsLittleEndian) Array.Reverse(res, 0, sizeof(int));
        File.WriteAllBytes(file, res);
    }

    public static void DeserializeBuffered(string file, IBinarySerializable serializable)
    {
        if (!File.Exists(file)) return;
        using var fileStream = File.OpenRead(file);
        using var zstdStream = new ZstdStream(fileStream, ZstdStreamMode.Decompress);
        using var bufferedStream = new BufferedStream(zstdStream);
        using var reader = new BinaryReader(bufferedStream);
        serializable.DeserializeBinary(reader, SerializationReason.Save);
    }

    public static void SerializeBuffered(string file, IBinarySerializable serializable)
    {
        using var fileStream = File.OpenWrite(file);
        using var zstdStream = new ZstdStream(fileStream, ZstdStreamMode.Compress);
        using var bufferedStream = new BufferedStream(zstdStream);
        using var writer = new BinaryWriter(bufferedStream);
        serializable.SerializeBinary(writer, SerializationReason.Save);
    }
}