// using BlockFactory.Serialization;
// using BlockFactory.World_.Chunk_;
// using OpenTK.Mathematics;
// using ZstdSharp;
//
// namespace BlockFactory.World_.Save;
//
// public class ChunkRegion : ISerializable
// {
//     public readonly Dictionary<Vector3i, ChunkData> ChunkDatas;
//     public readonly WorldSaveManager SaveManager;
//     public readonly Vector3i Pos;
//     public Task? LoadTask;
//     public Task? UnloadTask;
//
//     public ChunkRegion(WorldSaveManager saveManager, Vector3i pos)
//     {
//         SaveManager = saveManager;
//         Pos = pos;
//         ChunkDatas = new Dictionary<Vector3i, ChunkData>();
//     }
//
//     public void RunLoadTask()
//     {
//         LoadTask = Task.Run(Load);
//     }
//
//     public void RunUnloadTask()
//     {
//         UnloadTask = Task.Run(Unload);
//     }
//
//     private void Load()
//     {
//         string saveLocation = SaveManager.GetRegionSaveLocation(Pos);
//         if (File.Exists(saveLocation))
//         {
//             byte[] b = File.ReadAllBytes(saveLocation);
//             if (BitConverter.IsLittleEndian)
//             {
//                 Array.Reverse(b, 0, sizeof(int));
//             }
//
//             int uncompressedSize = BitConverter.ToInt32(b);
//             byte[] uncompressed = Zstd.Decompress(b, sizeof(int), b.Length - sizeof(int), uncompressedSize);
//             using MemoryStream stream = new MemoryStream(uncompressed);
//             using BinaryReader reader = new BinaryReader(stream);
//             CompoundTag tag = new CompoundTag();
//             tag.Read(reader);
//             FromTag(tag);
//         }
//     }
//
//     private void Unload()
//     {
//         using MemoryStream stream = new MemoryStream();
//         using BinaryWriter writer = new BinaryWriter(stream);
//         CompoundTag tag = ToTag();
//         tag.Write(writer);
//         byte[] uncompressed = stream.ToArray();
//         byte[] compressed = Zstd.Compress(uncompressed);
//         byte[] res = new byte[compressed.Length + sizeof(int)];
//         Array.Copy(compressed, 0, res, sizeof(int), compressed.Length);
//         BitConverter.TryWriteBytes(res, uncompressed.Length);
//         if (BitConverter.IsLittleEndian)
//         {
//             Array.Reverse(res, 0, sizeof(int));
//         }
//         File.WriteAllBytes(SaveManager.GetRegionSaveLocation(Pos), res);
//
//     }
//     
//     public void FromTag(CompoundTag tag)
//     {
//         ChunkDatas.Clear();
//         ListTag datas = tag.GetList("datas");
//         foreach (ITag pair in datas)
//         {
//             CompoundTag pairCompound = (CompoundTag)pair;
//             Vector3i pos = TagUtil.DeserializeVector3i(pairCompound.GetCompound("pos"));
//             ChunkData data = new ChunkData();
//             data.FromTag(pairCompound.GetCompound("data"));
//             ChunkDatas.Add(pos, data);
//         }
//     }
//
//     public CompoundTag ToTag()
//     {
//         CompoundTag tag = new CompoundTag();
//         ListTag datas = new ListTag();
//         foreach ((Vector3i pos, ChunkData data) in ChunkDatas)
//         {
//             CompoundTag pair = new CompoundTag();
//             pair.SetTag("pos", TagUtil.SerializeVector3(pos));
//             pair.SetTag("data", data.ToTag());
//             datas.Add(pair);
//         }
//         tag.SetTag("datas", datas);
//         return tag;
//     }
// }

