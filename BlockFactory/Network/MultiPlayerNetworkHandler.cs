using System.Collections.Concurrent;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Serialization;
using ENet.Managed;
using ZstdSharp;

namespace BlockFactory.Network;

public abstract class MultiPlayerNetworkHandler : INetworkHandler
{
    private readonly LogicalSide _logicalSide;
    private readonly Thread _networkThread;
    private readonly HashSet<ENetPeer> _peers;
    private readonly ConcurrentQueue<PacketReceiveQueueEntry> _receiveQueue;
    private readonly ConcurrentQueue<PacketSendQueueEntry> _sendQueue;
    public readonly ENetHost Host;
    private bool _shouldRun;

    public MultiPlayerNetworkHandler(LogicalSide side, ENetHost host)
    {
        Host = host;
        _sendQueue = new ConcurrentQueue<PacketSendQueueEntry>();
        _receiveQueue = new ConcurrentQueue<PacketReceiveQueueEntry>();
        _shouldRun = true;
        _networkThread = new Thread(ProcessNetwork);
        _logicalSide = side;
        _peers = new HashSet<ENetPeer>();
    }

    public void Update()
    {
        var cnt = _receiveQueue.Count;
        for (var i = 0; i < cnt; ++i)
        {
            if (!ShouldProcessPackets()) break;
            if (!_receiveQueue.TryDequeue(out var entry)) break;
            switch (entry.Type)
            {
                case 0:
                    OnPeerConnected(entry.Peer);
                    break;
                case 1:
                    OnPacketReceived(entry.Packet!, entry.Peer);
                    break;
                case 2:
                    OnPeerDisconnected(entry.Peer);
                    break;
            }
        }
    }

    public abstract bool ShouldStop();
    public abstract void SendPacket<T>(PlayerEntity? player, T packet) where T : class, IPacket;

    public void Start()
    {
        _networkThread.Start();
    }

    public void Dispose()
    {
        _shouldRun = false;
        _networkThread.Join();
        Host.Dispose();
    }

    protected virtual bool ShouldProcessPackets()
    {
        return true;
    }

    protected void EnqueueSendPacketInternal<T>(T packet, ENetPeer peer) where T : class, IPacket
    {
        _sendQueue.Enqueue(new PacketSendQueueEntry(packet, peer, NetworkRegistry.GetPacketFlags<T>(),
            NetworkRegistry.IsPacketCompressed<T>(), NetworkRegistry.GetPacketTypeId<T>()));
    }

    private void ProcessNetwork()
    {
        while (_shouldRun)
        {
            var first = false;
            while (true)
            {
                var evt = Host.Service(TimeSpan.FromMilliseconds(first ? 1 : 0));
                first = false;
                if (evt.Type == ENetEventType.None) break;
                ProcessEvent(evt);
            }

            var cnt = _sendQueue.Count;
            for (var i = 0; i < cnt; ++i)
            {
                if (!_sendQueue.TryDequeue(out var entry)) break;
                if (!_peers.Contains(entry.Peer)) continue;
                try
                {
                    var data = SerializePacket(entry.Packet, entry.Compressed, entry.Id);
                    entry.Peer.Send(0, data, entry.Flags);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }

    protected abstract void OnPeerConnected(ENetPeer peer);
    protected abstract void OnPeerDisconnected(ENetPeer peer);
    protected abstract void OnPacketReceived(IPacket packet, ENetPeer peer);


    private void Connect(ENetEvent evt)
    {
        _peers.Add(evt.Peer);
        _receiveQueue.Enqueue(new PacketReceiveQueueEntry(null, evt.Peer, 0));
    }

    private void Disconnect(ENetEvent evt)
    {
        _peers.Remove(evt.Peer);
        _receiveQueue.Enqueue(new PacketReceiveQueueEntry(null, evt.Peer, 2));
    }

    private void Receive(ENetEvent evt)
    {
        try
        {
            var packet = DeserializePacket(evt.Packet.Data.ToArray());
            if (packet.SupportsLogicalSide(_logicalSide))
                _receiveQueue.Enqueue(new PacketReceiveQueueEntry(packet, evt.Peer, 1));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


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

    private static IPacket DeserializePacket(byte[] b)
    {
        if (BitConverter.IsLittleEndian) Array.Reverse(b, 0, sizeof(int));

        var id = BitConverter.ToInt32(b);
        var p = NetworkRegistry.CreatePacket(id);
        byte[] readable;
        if (NetworkRegistry.IsPacketCompressed(id))
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(b, sizeof(int), sizeof(int));

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

    private static byte[] SerializePacket(IPacket packet, bool shouldBeCompressed, int id)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        packet.SerializeBinary(writer, SerializationReason.NetworkUpdate);
        var uncompressed = stream.ToArray();
        byte[] res;
        if (shouldBeCompressed)
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

        BitConverter.TryWriteBytes(res, id);
        if (BitConverter.IsLittleEndian) Array.Reverse(res, 0, sizeof(int));

        return res;
    }
}