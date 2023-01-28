using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_.Api;
using BlockFactory.Util.Math_;

namespace BlockFactory.World_.Chunk_
{
    public class ChunkNeighbourhood : IBlockStorage
    {
        public Vector3i CenterPos;
        public Chunk[,,] Chunks;
        public Chunk CenterChunk;
        public ChunkNeighbourhood(Chunk centerChunk)
        {
            CenterChunk = centerChunk;
            CenterPos = centerChunk.Pos;
            Chunks = new Chunk[3, 3, 3];
            Chunks[1, 1, 1] = centerChunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private Vector3i GetArrayChunkPos(Vector3i blockPos)
        {
            Vector3i chunkPos = blockPos.BitShiftRight(Chunk.SizeLog2);
            Vector3i deltaChunkPos = chunkPos - CenterPos;
            Vector3i arrChunkPos = deltaChunkPos + (1, 1, 1);
            return arrChunkPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static bool IsArrayChunkPosInside(Vector3i arrayChunkPos)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (arrayChunkPos[i] < 0 || arrayChunkPos[i] >= 3)
                {
                    return false;
                }
            }
            return true;
        }
        public BlockState GetBlockState(Vector3i pos)
        {
            Vector3i arrayChunkPos = GetArrayChunkPos(pos);
            if (IsArrayChunkPosInside(arrayChunkPos))
            {
                Chunk ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
                if (ch == null)
                {
                    return CenterChunk.World.GetBlockState(pos);
                }
                else
                {
                    return ch.GetBlockState(pos);
                }
            }
            else
            {
                return CenterChunk.World.GetBlockState(pos);
            }
        }

        public void SetBlockState(Vector3i pos, BlockState state)
        {
            Vector3i arrayChunkPos = GetArrayChunkPos(pos);
            if (IsArrayChunkPosInside(arrayChunkPos))
            {
                Chunk ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
                if (ch == null)
                {
                    CenterChunk.World.SetBlockState(pos, state);
                }
                else
                {
                    ch.SetBlockState(pos, state);
                }
            }
            else
            {
                CenterChunk.World.SetBlockState(pos, state);
            }
        }
    }
}
