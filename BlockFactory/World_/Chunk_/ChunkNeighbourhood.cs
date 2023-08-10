using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Util;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Chunk_;

public class ChunkNeighbourhood : IBlockStorage, IEntityStorage
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

    public BlockState GetBlockState(Vector3i pos)
    {
        var arrayChunkPos = GetArrayChunkPos(pos);
        var ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
        return ch?.GetBlockState(pos) ?? CenterChunk.World.GetBlockState(pos);
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        var arrayChunkPos = GetArrayChunkPos(pos);
        var ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
        if (ch == null)
            CenterChunk.World.SetBlockState(pos, state);
        else
            ch.SetBlockState(pos, state);
    }

    public bool AreAllNeighboursLoaded()
    {
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
        for (var k = 0; k < 3; ++k)
            if (Chunks[i, j, k] == null || !Chunks[i, j, k]!.Generated)
                return false;

        return true;
    }

    public bool AreAllNeighboursDecorated()
    {
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
        for (var k = 0; k < 3; ++k)
            if (Chunks[i, j, k] == null || !Chunks[i, j, k]!.Generated || !Chunks[i, j, k]!.Data.Decorated)
                return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private Vector3i GetArrayChunkPos(Vector3i blockPos)
    {
        var chunkPos = blockPos.BitShiftRight(Constants.ChunkSizeLog2);
        return ToArrayChunkPos(chunkPos);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private Vector3i ToArrayChunkPos(Vector3i chunkPos)
    {
        var deltaChunkPos = chunkPos - CenterPos;
        var arrChunkPos = deltaChunkPos + (1, 1, 1);
        return arrChunkPos;
    }

    // [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    // private static bool IsArrayChunkPosInside(Vector3i arrayChunkPos)
    // {
    //     for (var i = 0; i < 3; ++i)
    //         if (arrayChunkPos[i] < 0 || arrayChunkPos[i] >= 3)
    //             return false;
    //     return true;
    // }

    public void AddEntity(Entity entity, bool loaded = false)
    {
        var arrayChunkPos = ToArrayChunkPos(entity.Pos.ChunkPos);
        var ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
        if (ch == null)
        {
            CenterChunk.World.AddEntity(entity, loaded);
        }
        else
        {
            ch.AddEntity(entity, loaded);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        var arrayChunkPos = ToArrayChunkPos(entity.Pos.ChunkPos);
        var ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
        if (ch == null)
        {
            CenterChunk.World.RemoveEntity(entity);
        }
        else
        {
            ch.RemoveEntity(entity);
        }
    }

    public IEnumerable<Entity> GetInBoxEntityEnumerable(EntityPos p, Box3 b)
    {
        var min = p + b.Min;
        var max = p + b.Max;
        min.Fix();
        max.Fix();
        foreach (var chunkPos in new Box3i(min.ChunkPos, max.ChunkPos).InclusiveEnumerable())
        {
            foreach (var entity in GetChunk(chunkPos).GetInBoxEntityEnumerable(p, b))
            {
                yield return entity;
            }
        }
    }

    public Chunk GetChunk(Vector3i pos)
    {
        var arrayChunkPos = ToArrayChunkPos(pos);
        var ch = Chunks[arrayChunkPos.X, arrayChunkPos.Y, arrayChunkPos.Z];
        return ch ?? CenterChunk.World.GetOrLoadChunk(pos);
    }
}