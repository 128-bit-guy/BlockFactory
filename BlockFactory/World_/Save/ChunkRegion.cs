using BlockFactory.Serialization;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;
using BlockFactory.Util.Dependency;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;
using ZstdSharp;
using static BlockFactory.Serialization.VectorSerialization;

namespace BlockFactory.World_.Save;

public class ChunkRegion : ITagSerializable, IDependable
{
    public const int SizeLog2 = 4;
    public const int Size = 1 << SizeLog2;
    public const int Mask = Size - 1;
    public readonly Dictionary<Vector3i, ChunkData> ChunkDatas;
    public readonly WorldSaveManager SaveManager;
    public readonly Vector3i Pos;
    public Task? LoadTask;
    public Task? UnloadTask;
    private int _dependencyCount = 0;

    public ChunkRegion(WorldSaveManager saveManager, Vector3i pos)
    {
        SaveManager = saveManager;
        Pos = pos;
        ChunkDatas = new Dictionary<Vector3i, ChunkData>();
    }

    public void RunLoadTask()
    {
        LoadTask = Task.Run(Load);
    }

    public void RunUnloadTask()
    {
        UnloadTask = Task.Run(Unload);
    }

    private void Load()
    {
        var saveLocation = SaveManager.GetRegionSaveLocation(Pos);
        if (!File.Exists(saveLocation)) return;
        var b = File.ReadAllBytes(saveLocation);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b, 0, sizeof(int));
        }

        var uncompressedSize = BitConverter.ToInt32(b);
        var uncompressed = Zstd.Decompress(b, sizeof(int), b.Length - sizeof(int), uncompressedSize);
        using var stream = new MemoryStream(uncompressed);
        using var reader = new BinaryReader(stream);
        var tag = new DictionaryTag();
        tag.Read(reader);
        DeserializeFromTag(tag);
    }

    private void Unload()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var tag = SerializeToTag();
        tag.Write(writer);
        var uncompressed = stream.ToArray();
        var compressed = Zstd.Compress(uncompressed);
        var res = new byte[compressed.Length + sizeof(int)];
        Array.Copy(compressed, 0, res, sizeof(int), compressed.Length);
        BitConverter.TryWriteBytes(res, uncompressed.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(res, 0, sizeof(int));
        }

        string file = SaveManager.GetRegionSaveLocation(Pos);
        File.WriteAllBytes(file, res);
    }

    public DictionaryTag SerializeToTag()
    {
        var tag = new DictionaryTag();
        var datas = new ListTag(0, TagType.Dictionary);
        foreach (var (pos, data) in ChunkDatas)
        {
            var pair = new DictionaryTag();
            pair.Set("pos", pos.SerializeToTag());
            pair.Set("data", data.SerializeToTag());
            datas.Add(pair);
        }
        tag.Set("datas", datas);
        return tag;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        ChunkDatas.Clear();
        var datas = tag.Get<ListTag>("datas");
        foreach (var pair in datas.GetEnumerable<DictionaryTag>())
        {
            var pos = DeserializeV3I(pair.Get<ListTag>("pos"));
            var data = new ChunkData();
            data.DeserializeFromTag(pair.Get<DictionaryTag>("data"));
            ChunkDatas.Add(pos, data);
        }
    }

    public ChunkData GetOrCreateChunkData(Vector3i pos, out bool created)
    {
        if(ChunkDatas.TryGetValue(pos, out var data))
        {
            created = false;
            return data;
        }

        data = new ChunkData();
        ChunkDatas.Add(pos, data);
        created = true;
        return data;
    }

    public void EnsureLoading()
    {
        if (UnloadTask != null)
        {
            UnloadTask.Wait();
            UnloadTask = null;
        }
    }

    public void EnsureLoaded()
    {
        EnsureLoading();
        if (LoadTask != null)
        {
            LoadTask.Wait();
            LoadTask = null;
        }
    }

    public void EnsureUnloading()
    {
        if (LoadTask != null)
        {
            LoadTask.Wait();
            LoadTask = null;
        }

        if (UnloadTask == null)
        {
            RunUnloadTask();
        }
    }

    public ref int DependencyCount => ref _dependencyCount;
}

