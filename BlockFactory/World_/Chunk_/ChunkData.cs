using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Chunk_
{
    public class ChunkData : IBlockStorage
    {
        private ushort[,,] _blocks;
        private byte[,,] _rotations;
        public ChunkGenerationLevel _generationLevel;
        public ChunkData()
        {
            _blocks = new ushort[Chunk.Size, Chunk.Size, Chunk.Size];
            _rotations = new byte[Chunk.Size, Chunk.Size, Chunk.Size];
            _generationLevel = ChunkGenerationLevel.Exists;
        }

        public ChunkData(BinaryReader reader) : this()
        {
            for (int i = 0; i < Chunk.Size; ++i)
            {
                for (int j = 0; j < Chunk.Size; ++j)
                {
                    for (int k = 0; k < Chunk.Size; ++k)
                    {
                        _blocks[i, j, k] = reader.ReadUInt16();
                        _rotations[i, j, k] = reader.ReadByte();
                    }
                }
            }
            _generationLevel = (ChunkGenerationLevel)reader.ReadByte();
        }

        public BlockState GetBlockState(Vector3i pos)
        {
            ushort block = _blocks[pos.X, pos.Y, pos.Z];
            CubeRotation rotation = CubeRotation.Rotations[_rotations[pos.X, pos.Y, pos.Z]];
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

        public void Write(BinaryWriter writer)
        {
            for (int i = 0; i < Chunk.Size; ++i)
            {
                for (int j = 0; j < Chunk.Size; ++j)
                {
                    for (int k = 0; k < Chunk.Size; ++k)
                    {
                        writer.Write(_blocks[i, j, k]);
                        writer.Write(_rotations[i, j, k]);
                    }
                }
            }
            writer.Write((byte)_generationLevel);
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
    }
}
