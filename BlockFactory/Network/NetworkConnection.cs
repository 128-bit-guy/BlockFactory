using System.Collections.Concurrent;
using System.Net.Sockets;
using BlockFactory.Game;
using BlockFactory.Util;
using Microsoft.IO;
using ZstdSharp;

namespace BlockFactory.Network;

/// <summary>
///     Handles connection between server and client
/// </summary>
public class NetworkConnection : IDisposable
{
    private static readonly RecyclableMemoryStreamManager StreamManager = new();
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<IPacket> _sendQueue;
    public readonly ConcurrentQueue<IPacket> ReceiveQueue;

    /// <summary>
    ///     Socket which is used for this connection
    /// </summary>
    public readonly Socket Socket;

    private Task? _currentReceiveTask;
    private Task? _currentSendTask;
    private int _working;
    public GameInstance? GameInstance;
    public object? SideObject;

    /// <summary>
    ///     Constructs network connection from socket
    /// </summary>
    /// <param name="socket">Socket which will be used for this connection</param>
    public NetworkConnection(Socket socket)
    {
        Socket = socket;
        _sendQueue = new ConcurrentQueue<IPacket>();
        _semaphore = new SemaphoreSlim(0);
        _cancellationTokenSource = new CancellationTokenSource();
        _working = 0;
        OnError = _ => { };
        OnStop = () => { };
        ReceiveQueue = new ConcurrentQueue<IPacket>();
    }

    public bool Errored { get; private set; }
    public bool Working => _working != 0;
    public Exception? LastError { get; private set; }

    /// <summary>
    ///     Disposes resources used for this network connection, including socket.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    public event Action<Exception> OnError;
    public event Action OnStop;

    ~NetworkConnection()
    {
        Dispose(false);
    }

    private void Error(Exception exception)
    {
        if (ExceptionUtils.NotCancelledException(exception))
        {
            OnError(exception);
            Errored = true;
            LastError = exception;
            Stop();
        }
    }

    /// <summary>
    ///     Starts all network tasks
    /// </summary>
    public void Start()
    {
        _working = 1;
        _currentSendTask = Task.Run(SendPackets);
        _currentReceiveTask = Task.Run(ReceivePackets);
    }

    /// <summary>
    ///     Sends packet to other side.
    /// </summary>
    /// <param name="packet">Packet, which will be sent</param>
    public void SendPacket(IPacket packet)
    {
        _sendQueue.Enqueue(packet);
        _semaphore.Release();
    }

    public void Flush()
    {
        SendPacket(null!);
    }

    /// <summary>
    ///     Stops all network tasks and waits for their completion. May be called from any thread
    /// </summary>
    public void Stop()
    {
        if (Interlocked.Exchange(ref _working, 0) != 0)
        {
            _cancellationTokenSource.Cancel();
            OnStop();
        }
    }

    private static void WritePacket(IPacket packet, BinaryWriter writer)
    {
        var id = NetworkRegistry.GetPacketId(packet.GetType());
        writer.Write(id);
        NetworkRegistry.WritePacket(packet, writer);
    }

    private void HandlePacket(BinaryReader reader)
    {
        var id = reader.ReadInt32();
        var packet = NetworkRegistry.ReadPacket(reader, id);
        if (!packet.SupportsGameKind(GameInstance.Kind))
            throw new InvalidOperationException(
                $"Other side sent packet {packet.GetType().Name} which is not supported on this side");
        ReceiveQueue.Enqueue(packet);
    }

    private async ValueTask SendPacketList(List<IPacket> packetList)
    {
        using var stream = StreamManager.GetStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(packetList.Count);
        foreach (var packet in packetList)
            WritePacket(packet, writer);
        var res = stream.ToArray();
        var compressed = Zstd.Compress(res);
        var toSend = new byte[compressed.Length + 2 * sizeof(int)];
        Array.Copy(compressed, 0, toSend, 2 * sizeof(int), compressed.Length);
        BitConverter.TryWriteBytes(toSend, compressed.Length);
        BitConverter.TryWriteBytes(toSend.AsSpan()[sizeof(int)..], res.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(toSend, 0, sizeof(int));
            Array.Reverse(toSend, sizeof(int), sizeof(int));
        }

        var toSendPos = 0;
        while (toSendPos < toSend.Length)
            toSendPos += await Socket.SendAsync(
                new ArraySegment<byte>(toSend, toSendPos, toSend.Length - toSendPos),
                0,
                _cancellationTokenSource.Token
            );
    }

    private async Task SendPackets()
    {
        try
        {
            var packets = new List<IPacket>();
            while (Working)
            {
                await _semaphore.WaitAsync(_cancellationTokenSource.Token);
                if (_sendQueue.TryDequeue(out var packet))
                {
                    if (packet == null)
                    {
                        await SendPacketList(packets);
                        packets.Clear();
                    }
                    else
                    {
                        packets.Add(packet);
                    }
                }
                else
                {
                    throw new Exception($"{nameof(_sendQueue)} is empty. This should never happen.");
                }
            }
        }
        catch (Exception ex)
        {
            Error(ex);
        }
    }

    private async ValueTask ReceivePacketList()
    {
        //Stopwatch sw = new Stopwatch();
        //sw.Start();
        var lengths = new byte[2 * sizeof(int)];
        var lengthsPos = 0;
        while (lengthsPos < lengths.Length)
            lengthsPos += await Socket.ReceiveAsync(
                new ArraySegment<byte>(lengths, lengthsPos, lengths.Length - lengthsPos),
                0,
                _cancellationTokenSource.Token
            );
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengths, 0, sizeof(int));
            Array.Reverse(lengths, sizeof(int), sizeof(int));
        }

        var compressedLength = BitConverter.ToInt32(lengths);
        var uncompressedLength = BitConverter.ToInt32(lengths.AsSpan()[sizeof(int)..]);
        var compressed = new byte[compressedLength];
        var compressedPos = 0;
        while (compressedPos < compressed.Length)
            compressedPos += await Socket.ReceiveAsync(
                new ArraySegment<byte>(compressed, compressedPos, compressed.Length - compressedPos),
                0,
                _cancellationTokenSource.Token
            );
        var uncompressed = Zstd.Decompress(compressed, uncompressedLength);
        using var stream = StreamManager.GetStream(uncompressed);
        using var reader = new BinaryReader(stream);
        var cnt = reader.ReadInt32();
        for (var i = 0; i < cnt; ++i)
            HandlePacket(reader);
    }

    private async Task ReceivePackets()
    {
        try
        {
            while (Working) await ReceivePacketList();
        }
        catch (Exception ex)
        {
            Error(ex);
        }
    }

    private void Dispose(bool _)
    {
        _semaphore.Dispose();
        Socket.Dispose();
        _cancellationTokenSource.Dispose();
    }

    public void ProcessInGamePackets()
    {
        var cnt = ReceiveQueue.Count;
        for (var i = 0; i < cnt; ++i)
        {
            if (ReceiveQueue.TryDequeue(out var packet))
            {
                if (packet is not IInGamePacket inGamePacket)
                    throw new InvalidOperationException(
                        $"Can not handle not in game packet {packet.GetType().Name} during game");
                inGamePacket.Process(this);
            }
            else
            {
                break;
            }
        }
    }

    public T AwaitPacket<T>() where T : IPacket
    {
        var wait = new SpinWait();
        IPacket packet;
        DateTime begin = DateTime.UtcNow;
        while (!ReceiveQueue.TryDequeue(out packet))
        {
            if (DateTime.UtcNow - begin > TimeSpan.FromSeconds(1))
            {
                throw new TimeoutException($"Awaiting packet of type {typeof(T).Name} timed out");
            }
            wait.SpinOnce();
        }

        if (packet is not T tPacket)
        {
            throw new InvalidOperationException(
                $"Received packet of type {packet.GetType()} when awaiting packet of type {typeof(T).Name}");
        }

        return tPacket;
    }
}