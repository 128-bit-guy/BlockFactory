using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Util.Math_;
using OpenTK.Mathematics;
using BlockFactory.World_;

namespace BlockFactory.Block_
{
    public struct BlockState : IEquatable<BlockState>
    {
        public Block Block;
        public CubeRotation Rotation;
        public BlockState(Block block, CubeRotation rotation) {
            Block = block;
            Rotation = rotation;
        }

        public BlockState(BinaryReader reader)
        {
            Block = Blocks.Registry[reader.ReadUInt16()];
            Rotation = CubeRotation.Rotations[reader.ReadByte()];
        }

        public bool Equals(BlockState other)
        {
            return Block.Equals(other.Block) && Rotation.Equals(other.Rotation);
        }

        public override bool Equals(object? obj)
        {
            return obj is BlockState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Block, Rotation);
        }

        public static bool operator ==(BlockState left, BlockState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlockState left, BlockState right)
        {
            return !left.Equals(right);
        }

        public readonly void Write(BinaryWriter writer)
        {
            writer.Write((ushort)Block.Id);
            writer.Write(Rotation.Ordinal);
        }
    }
}
