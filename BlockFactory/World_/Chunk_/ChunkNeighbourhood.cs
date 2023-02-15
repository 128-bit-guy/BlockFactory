using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Chunk_;

public class ChunkNeighbourhood : IBlockStorage
{
    public Chunk CenterChunk;
    public Vector3i CenterPos;
    public Chunk?[,,] Chunks;

    public ChunkNeighbourhood(Chunk centerChunk)
    {
        CenterChunk = centerChunk;
        CenterPos = centerChunk.Pos;
        Chunks = new Chunk?[3, 3, 3];
        Chunks[1, 1, 1] = centerChunk;
    }

    public bool AreAllNeighboursLoaded()
    {
        for (var i = 0; i < 3; ++i)
        {
            for (var j = 0; j < 3; ++j)
            {
                for (var k = 0; k < 3; ++k)
                {
                    if (Chunks[i, j, k] == null || !Chunks[i, j, k].Generated)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public BlockState GetBlockState(Vector3i pos)
    {
        var arrayChunkPos = GetArrayChunkPos(pos);
        if (IsArrayChunkPosInside(arrayChunkPos))
        {
            var ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
            if (ch == null)
                return CenterChunk.World.GetBlockState(pos);
            return ch.GetBlockState(pos);
        }

        return CenterChunk.World.GetBlockState(pos);
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        var arrayChunkPos = GetArrayChunkPos(pos);
        if (IsArrayChunkPosInside(arrayChunkPos))
        {
            var ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
            if (ch == null)
                CenterChunk.World.SetBlockState(pos, state);
            else
                ch.SetBlockState(pos, state);
        }
        else
        {
            CenterChunk.World.SetBlockState(pos, state);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private Vector3i GetArrayChunkPos(Vector3i blockPos)
    {
        var chunkPos = blockPos.BitShiftRight(Constants.ChunkSizeLog2);
        var deltaChunkPos = chunkPos - CenterPos;
        var arrChunkPos = deltaChunkPos + (1, 1, 1);
        return arrChunkPos;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool IsArrayChunkPosInside(Vector3i arrayChunkPos)
    {
        for (var i = 0; i < 3; ++i)
            if (arrayChunkPos[i] < 0 || arrayChunkPos[i] >= 3)
                return false;
        return true;
    }
}