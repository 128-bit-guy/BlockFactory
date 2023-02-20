using System.Collections.Concurrent;
using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;
using BlockFactory.World_;
using ZstdSharp;

namespace BlockFactory.Game;

public class GameInstance : IDisposable
{
    private readonly ConcurrentQueue<Action> Actions = new();
    public readonly GameKind Kind;
    public readonly string SaveLocation;
    private DateTime _nextTickTime;
    public Thread MainThread;
    public INetworkHandler NetworkHandler = null!;
    public Random Random;
    public ISideHandler SideHandler = null!;
    public World World;
    public PlayerManager PlayerManager;
    public readonly string PlayerDataLocation;

    public GameInstance(GameKind kind, Thread mainThread, int seed, string saveLocation)
    {
        Kind = kind;
        MainThread = mainThread;
        Random = new Random();
        SaveLocation = saveLocation;
        World = new World(this, seed, saveLocation);
        PlayerManager = new PlayerManager(this);
        PlayerDataLocation = Path.Combine(SaveLocation, "players.dat");
        if (kind.DoesProcessLogic())
        {
            LoadGlobalData();
        }
    }

    public void Dispose()
    {
        if (Kind.DoesProcessLogic())
        {
            SaveGlobalData();
        }

        World.Dispose();
    }

    public void Init()
    {
        _nextTickTime = DateTime.UtcNow;
    }


    private void Tick()
    {
        World.Tick();
        if (Kind.IsNetworked())
        {
            if (Kind.DoesProcessLogic())
                foreach (var connection in NetworkHandler.GetAllConnections())
                    connection.Flush();
            else
                NetworkHandler.GetServerConnection().Flush();
        }
    }

    public bool Update()
    {
        ProcessScheduled();
        if (DateTime.UtcNow >= _nextTickTime)
        {
            _nextTickTime += Constants.TickPeriod;
            Tick();
            return true;
        }

        return false;
    }

    public void ProcessScheduled()
    {
        var cnt = Actions.Count;
        for (var i = 0; i < cnt; ++i)
            if (Actions.TryDequeue(out var action))
                action();
    }

    public void EnqueueWork(Action action)
    {
        Actions.Enqueue(action);
    }

    public void LoadGlobalData()
    {
        var saveLocation = PlayerDataLocation;
        if (!File.Exists(saveLocation)) return;
        var b = File.ReadAllBytes(saveLocation);
        if (BitConverter.IsLittleEndian) Array.Reverse(b, 0, sizeof(int));

        var uncompressedSize = BitConverter.ToInt32(b);
        var uncompressed = Zstd.Decompress(b, sizeof(int), b.Length - sizeof(int), uncompressedSize);
        using var stream = new MemoryStream(uncompressed);
        using var reader = new BinaryReader(stream);
        var tag = new DictionaryTag();
        tag.Read(reader);
        ((ITagSerializable)PlayerManager).DeserializeFromTag(tag);
    }

    public void SaveGlobalData()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var tag = ((ITagSerializable)PlayerManager).SerializeToTag();
        tag.Write(writer);
        var uncompressed = stream.ToArray();
        var compressed = Zstd.Compress(uncompressed);
        var res = new byte[compressed.Length + sizeof(int)];
        Array.Copy(compressed, 0, res, sizeof(int), compressed.Length);
        BitConverter.TryWriteBytes(res, uncompressed.Length);
        if (BitConverter.IsLittleEndian) Array.Reverse(res, 0, sizeof(int));
        var file = PlayerDataLocation;
        File.WriteAllBytes(file, res);
    }
}