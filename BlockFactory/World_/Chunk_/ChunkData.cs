using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Block_.Instance;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Init;
using BlockFactory.Serialization;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;
using BlockFactory.Util;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Chunk_;

public class ChunkData : IBlockStorage, ITagSerializable, IBinarySerializable
{
    private ushort[,,] _blocks;
    private byte[,,] _rotations;
    public bool Decorated;
    public Dictionary<long, Entity> EntitiesInChunk;
    public Dictionary<Vector3i, BlockInstance> BlockInstancesInChunk;
    private List<(EntityType, DictionaryTag)>? _entityTags;
    private List<(Vector3i, DictionaryTag)>? _blockInstanceTags;

    public ChunkData()
    {
        _blocks = new ushort[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize];
        _rotations = new byte[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize];
        Decorated = false;
        EntitiesInChunk = new Dictionary<long, Entity>();
        BlockInstancesInChunk = new Dictionary<Vector3i, BlockInstance>();
    }

    public void SerializeToBinaryWriter(BinaryWriter writer)
    {
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            writer.Write(_blocks[i, j, k]);
            writer.Write(_rotations[i, j, k]);
        }

        writer.Write7BitEncodedInt(_entityTags!.Count);
        foreach (var (type, tag) in _entityTags)
        {
            writer.Write(type.Id);
            tag.Write(writer);
        }
        
        writer.Write7BitEncodedInt(_blockInstanceTags!.Count);
        foreach (var (pos, tag) in _blockInstanceTags)
        {
            pos.Write(writer);
            tag.Write(writer);
        }
    }

    public void DeserializeFromBinaryReader(BinaryReader reader)
    {
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            _blocks[i, j, k] = reader.ReadUInt16();
            _rotations[i, j, k] = reader.ReadByte();
        }

        _entityTags = new List<(EntityType, DictionaryTag)>();
        var cnt = reader.Read7BitEncodedInt();
        for (var i = 0; i < cnt; ++i)
        {
            var t = Entities.Registry[reader.ReadInt32()];
            var tag = new DictionaryTag();
            tag.Read(reader);
            _entityTags.Add((t, tag));
        }

        _blockInstanceTags = new List<(Vector3i, DictionaryTag)>();
        var instanceCnt = reader.Read7BitEncodedInt();
        for (int i = 0; i < instanceCnt; i++)
        {
            var pos = NetworkUtils.ReadVector3i(reader);
            var tag = new DictionaryTag();
            tag.Read(reader);
            _blockInstanceTags.Add((pos, tag));
        }
    }

    public BlockState GetBlockState(Vector3i pos)
    {
        var block = _blocks[pos.X, pos.Y, pos.Z];
        var rotation = CubeRotation.Rotations[_rotations[pos.X, pos.Y, pos.Z]];
        return new BlockState(Blocks.Registry[block], rotation, BlockInstancesInChunk!.GetValueOrDefault(pos, null));
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        _blocks[pos.X, pos.Y, pos.Z] = (ushort)state.Block.Id;
        _rotations[pos.X, pos.Y, pos.Z] = state.Rotation.Ordinal;
        BlockInstancesInChunk.Remove(pos);
        if (state.Instance != null)
        {
            state.Instance.Pos = pos;
            BlockInstancesInChunk[pos] = state.Instance;
        }
    }

    public DictionaryTag SerializeToTag()
    {
        var tag = new DictionaryTag();
        tag.Set("blocks", new ChunkBlockDataTag(_blocks));
        tag.Set("rotations", new ChunkRotationDataTag(_rotations));
        tag.SetValue("decorated", Decorated);
        var listTag = new ListTag(0, TagType.Dictionary);
        foreach (var entity in EntitiesInChunk.Values)
        {
            if (entity.Type == Entities.Player) continue;
            var dictTag = new DictionaryTag();
            dictTag.SetValue("type", entity.Type.Id);
            dictTag.Set("tag", ((ITagSerializable)entity).SerializeToTag());
            listTag.Add(dictTag);
        }

        tag.Set("entities", listTag);

        var blockInstancesListTag = new ListTag(0, TagType.Dictionary);
        foreach (var instance in BlockInstancesInChunk.Values)
        {
            var dictTag = new DictionaryTag();
            dictTag.Set("pos", instance.Pos.SerializeToTag());
            dictTag.Set("tag", ((ITagSerializable)instance).SerializeToTag());
            blockInstancesListTag.Add(dictTag);
        }
        
        tag.Set("block_instances", blockInstancesListTag);

        return tag;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        _blocks = tag.Get<ChunkBlockDataTag>("blocks").Data;
        _rotations = tag.Get<ChunkRotationDataTag>("rotations").Data;
        Decorated = tag.GetValue<bool>("decorated");
        var listTag = tag.Get<ListTag>("entities");
        EntitiesInChunk.Clear();
        foreach (var dictTag in listTag.GetEnumerable<DictionaryTag>())
        {
            var type = Entities.Registry[dictTag.GetValue<int>("type")];
            var entity = type.EntityCreator();
            ((ITagSerializable)entity).DeserializeFromTag(dictTag.Get<DictionaryTag>("tag"));
            EntitiesInChunk.Add(entity.Id, entity);
        }
        BlockInstancesInChunk.Clear();
        var blockInstancesListTag = tag.Get<ListTag>("block_instances");
        foreach (var dictionaryTag in blockInstancesListTag.GetEnumerable<DictionaryTag>())
        {
            var pos = VectorSerialization.DeserializeV3I(dictionaryTag.Get<ListTag>("pos"));
            var blockInstance = Blocks.Registry[_blocks[pos.X, pos.Y, pos.Z]].CreateInstance();
            if (blockInstance == null) continue;
            ((ITagSerializable)blockInstance).DeserializeFromTag(dictionaryTag.Get<DictionaryTag>("tag"));
            BlockInstancesInChunk.Add(pos, blockInstance);
        }
    }

    private List<(EntityType, DictionaryTag)> GetEntityTags(PlayerEntity e)
    {
        return (from entity in EntitiesInChunk.Values
            where entity != e
            let tag = ((ITagSerializable)entity).SerializeToTag()
            select (entity.Type, tag)).ToList();
    }

    private List<(Vector3i, DictionaryTag)> GetBlockInstanceTags()
    {
        return (from blockInstance in BlockInstancesInChunk.Values
            let tag = ((ITagSerializable)blockInstance).SerializeToTag()
            select (blockInstance.Pos, tag)).ToList();
    }

    public ChunkData ConvertForSending(PlayerEntity entity)
    {
        ChunkData res = new()
        {
            _blocks = (ushort[,,])_blocks.Clone(),
            _rotations = (byte[,,])_rotations.Clone(),
            _entityTags = GetEntityTags(entity),
            _blockInstanceTags = GetBlockInstanceTags()
        };
        return res;
    }

    public void UnConvertFromSending()
    {
        foreach (var (type, tag) in _entityTags!)
        {
            var entity = type.EntityCreator();
            ((ITagSerializable)entity).DeserializeFromTag(tag);
            EntitiesInChunk.Add(entity.Id, entity);
        }

        foreach (var (pos, tag) in _blockInstanceTags)
        {
            var blockInstance = Blocks.Registry[_blocks[pos.X, pos.Y, pos.Z]].CreateInstance()!;
            ((ITagSerializable)blockInstance).DeserializeFromTag(tag);
            BlockInstancesInChunk.Add(pos, blockInstance);
        }

        _entityTags = null;
    }
}