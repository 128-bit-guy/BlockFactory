using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Chunk_;

public class ChunkData : IBlockStorage, ITagSerializable, IBinarySerializable
{
    private ushort[,,] _blocks;
    public ChunkGenerationLevel _generationLevel;
    private byte[,,] _rotations;

    public ChunkData()
    {
        _blocks = new ushort[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize];
        _rotations = new byte[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize];
        _generationLevel = ChunkGenerationLevel.Exists;
    }

    public BlockState GetBlockState(Vector3i pos)
    {
        var block = _blocks[pos.X, pos.Y, pos.Z];
        var rotation = CubeRotation.Rotations[_rotations[pos.X, pos.Y, pos.Z]];
        return new BlockState(Blocks.Registry[block], rotation);
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        _blocks[pos.X, pos.Y, pos.Z] = (ushort)state.Block.Id;
        _rotations[pos.X, pos.Y, pos.Z] = state.Rotation.Ordinal;
    }

    public ChunkData Clone()
    {
        ChunkData res = new()
        {
            _blocks = (ushort[,,])_blocks.Clone(),
            _rotations = (byte[,,])_rotations.Clone(),
            _generationLevel = _generationLevel
        };
        return res;
    }

    // public void FromTag(CompoundTag tag)
    // {
    //     tag.ApplyChunkBlockData("blocks", _blocks);
    //     tag.ApplyChunkRotationData("rotations", _rotations);
    //     _generationLevel = (ChunkGenerationLevel)tag.GetByte("generationLevel");
    // }
    //
    // public CompoundTag ToTag()
    // {
    //     CompoundTag tag = new CompoundTag();
    //     tag.SetChunkBlockData("blocks", _blocks);
    //     tag.SetChunkRotationData("rotations", _rotations);
    //     tag.SetByte("generationLevel", (byte)_generationLevel);
    //     return tag;
    // }
    public DictionaryTag SerializeToTag()
    {
        DictionaryTag tag = new DictionaryTag();
        tag.Set("blocks", new ChunkBlockDataTag(_blocks));
        tag.Set("rotations", new ChunkRotationDataTag(_rotations));
        tag.SetValue("generationLevel", (int)_generationLevel);
        return tag;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        _blocks = tag.Get<ChunkBlockDataTag>("blocks").Data;
        _rotations = tag.Get<ChunkRotationDataTag>("rotations").Data;
        _generationLevel = (ChunkGenerationLevel)tag.GetValue<int>("generationLevel");
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

        writer.Write((byte)_generationLevel);
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

        _generationLevel = (ChunkGenerationLevel)reader.ReadByte();
    }
}