using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Side_;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;
using BlockFactory.World_.Chunk_;

namespace BlockFactory.Client.Render.World_
{
    [ExclusiveTo(Side.Client)]
    public class ChunkRendererNeighbourhood : IBlockReader
    {
        public Vector3i CenterPos;
        public Chunk?[,,] Chunks;
        public int LoadedChunkCnt;
        public ChunkRendererNeighbourhood(Vector3i centerPos)
        {
            CenterPos = centerPos;
            Chunks = new Chunk?[3, 3, 3];
            LoadedChunkCnt = 0;
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


        public void OnChunkLoaded(Chunk chunk)
        {
            Vector3i arrayChunkPos = chunk.Pos - CenterPos + new Vector3i(1, 1, 1);
            if (!IsArrayChunkPosInside(arrayChunkPos)) {
                throw new ArgumentOutOfRangeException(nameof(chunk), "Loaded chunk is out of range");
            }
            Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z] = chunk;
            ++LoadedChunkCnt;
        }

        public void OnChunkUnloaded(Vector3i pos)
        {
            Vector3i arrayChunkPos = pos - CenterPos + new Vector3i(1, 1, 1);
            if (!IsArrayChunkPosInside(arrayChunkPos))
            {
                throw new ArgumentOutOfRangeException(nameof(pos), "Unloaded chunk pos is out of range");
            }
            --LoadedChunkCnt;
        }

        public BlockState GetBlockState(Vector3i pos)
        {
            Vector3i arrayChunkPos = GetArrayChunkPos(pos);
            if (IsArrayChunkPosInside(arrayChunkPos))
            {
                Chunk? ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
                if (ch == null)
                {
                    throw new ArgumentException("Requested block is not loaded");
                }
                else
                {
                    return ch.GetBlockState(pos);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pos), "Requested block is out of range");
            }
        }
    }
}
