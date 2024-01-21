using System.Runtime.CompilerServices;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_.Serialization;

public class ChunkRegion : IBinarySerializable
{
    public const int SizeLog2 = 4;
    public const int Size = 1 << SizeLog2;
    public const int Mask = Size - 1;

    private readonly ChunkData?[] _chunks = new ChunkData?[1 << (3 * SizeLog2)];
    public readonly Vector3D<int> Position;
    public readonly WorldSaveManager SaveManager;
    public int DependencyCount = 0;
    public Task? LoadTask;
    public Task? UnloadTask;

    public ChunkRegion(Vector3D<int> position, WorldSaveManager saveManager)
    {
        Position = position;
        SaveManager = saveManager;
    }

    public ChunkData? GetChunk(Vector3D<int> pos)
    {
        return _chunks[GetArrIndex(pos)];
    }

    public void SetChunk(Vector3D<int> pos, ChunkData chunk)
    {
        _chunks[GetArrIndex(pos)] = chunk;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & Mask) | (((pos.Y & Mask) | ((pos.Z & Mask) << SizeLog2)) << SizeLog2);
    }

    private string GetSaveFile()
    {
        return Path.Combine(SaveManager.SaveLocation, $"region_{Position.X}_{Position.Y}_{Position.Z}.dat");
    }

    private void Load()
    {
        var saveFile = GetSaveFile();
        if (!File.Exists(saveFile)) return;
        BinaryIO.Deserialize(saveFile, this);
    }

    private void Unload()
    {
        var saveFile = GetSaveFile();
        BinaryIO.Serialize(saveFile, this);
    }

    public void StartLoadTask()
    {
        LoadTask = Task.Run(Load);
    }

    public void StartUnloadTask()
    {
        UnloadTask = Task.Run(Unload);
    }

    // public DictionaryTag SerializeToTag(SerializationReason reason)
    // {
    //     var res = new DictionaryTag();
    //     var resList = new ListTag();
    //     for (var i = 0; i < _chunks.Length; ++i)
    //     {
    //         if (_chunks[i] == null) continue;
    //         var tag = _chunks[i]!.SerializeToTag(reason);
    //         tag.SetValue("index", i);
    //         resList.Add(tag);
    //     }
    //     res.Set("chunks", resList);
    //     return res;
    // }
    //
    // public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    // {
    //     Array.Fill(_chunks, null);
    //     var list = tag.Get<ListTag>("chunks");
    //
    //     foreach (var dict in list.GetEnumerable<DictionaryTag>())
    //     {
    //         var i = dict.GetValue<int>("index");
    //         var data = new ChunkData();
    //         data.DeserializeFromTag(dict, reason);
    //         _chunks[i] = data;
    //     }
    // }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        var cnt = 0;
        foreach (var data in _chunks)
        {
            if (data != null)
            {
                ++cnt;
            }
        }

        writer.Write(cnt);
        for (var i = 0; i < _chunks.Length; ++i)
        {
            if (_chunks[i] == null) continue;
            writer.Write(i);
            _chunks[i]!.SerializeBinary(writer, reason);
        }
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        Array.Fill(_chunks, null);
        var cnt = reader.ReadInt32();
        for (var i = 0; i < cnt; ++i)
        {
            var ind = reader.ReadInt32();
            _chunks[ind] = new ChunkData();
            _chunks[ind]!.DeserializeBinary(reader, reason);
        }
    }
}