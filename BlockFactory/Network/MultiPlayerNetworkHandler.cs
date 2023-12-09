using BlockFactory.Entity_;
using BlockFactory.Serialization;
using ENet.Managed;
using ZstdSharp;

namespace BlockFactory.Network;

public abstract class MultiPlayerNetworkHandler : INetworkHandler
{
    public readonly ENetHost Host;

    public MultiPlayerNetworkHandler(ENetHost host)
    {
        Host = host;
    }

    public void Update()
    {
        var first = false;
        while (true)
        {
            var evt = Host.Service(TimeSpan.FromMilliseconds(first ? 1 : 0));
            first = false;
            if (evt.Type == ENetEventType.None) break;
            // Console.WriteLine($"Network event of type: {evt.Type}");
            ProcessEvent(evt);
        }
    }

    public abstract bool ShouldStop();
    public abstract void SendPacket<T>(PlayerEntity? player, T packet) where T : class, IPacket;


    protected abstract void Connect(ENetEvent evt);

    protected abstract void Disconnect(ENetEvent evt);

    protected abstract void Receive(ENetEvent evt);


    private void ProcessEvent(ENetEvent evt)
    {
        if (evt.Type == ENetEventType.Connect)
        {
            Connect(evt);
        }
        else if (evt.Type == ENetEventType.Disconnect)
        {
            Disconnect(evt);
        }
        else
        {
            Receive(evt);
            evt.Packet.Destroy();
        }
    }

    protected static IPacket DeserializePacket(byte[] b)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b, 0, sizeof(int));
        }

        var id = BitConverter.ToInt32(b);
        var p = NetworkRegistry.CreatePacket(id);
        byte[] readable;
        if (NetworkRegistry.IsPacketCompressed(id))
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b, sizeof(int), sizeof(int));
            }

            var uncompressedSize = BitConverter.ToInt32(b.AsSpan()[sizeof(int)..]);
            readable = Zstd.Decompress(b, 2 * sizeof(int), b.Length - 2 * sizeof(int), uncompressedSize);
        }
        else
        {
            readable = new byte[b.Length - sizeof(int)];
            Array.Copy(b, sizeof(int), readable, 0, b.Length - sizeof(int));
        }

        using var stream = new MemoryStream(readable);
        using var reader = new BinaryReader(stream);
        p.DeserializeBinary(reader, SerializationReason.NetworkUpdate);
        return p;
    }

    protected static byte[] SerializePacket<T>(T packet) where T : class, IPacket
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        packet.SerializeBinary(writer, SerializationReason.NetworkUpdate);
        var uncompressed = stream.ToArray();
        byte[] res;
        if (NetworkRegistry.IsPacketCompressed<T>())
        {
            var compressed = Zstd.Compress(uncompressed);
            res = new byte[compressed.Length + 2 * sizeof(int)];
            Array.Copy(compressed, 0, res, 2 * sizeof(int), compressed.Length);
            BitConverter.TryWriteBytes(res.AsSpan()[sizeof(int)..], uncompressed.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(res, sizeof(int), sizeof(int));
        }
        else
        {
            res = new byte[uncompressed.Length + sizeof(int)];
            Array.Copy(uncompressed, 0, res, sizeof(int), uncompressed.Length);
        }

        BitConverter.TryWriteBytes(res, NetworkRegistry.GetPacketTypeId<T>());
        if (BitConverter.IsLittleEndian) Array.Reverse(res, 0, sizeof(int));

        return res;
    }

    public void Dispose()
    {
        Host.Dispose();
    }
}