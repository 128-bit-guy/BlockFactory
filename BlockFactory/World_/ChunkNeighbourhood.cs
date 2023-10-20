﻿using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class ChunkNeighbourhood : IChunkStorage, IBlockWorld
{
    private readonly Chunk?[] _neighbours = new Chunk?[4 * 4 * 4];
    public readonly Chunk Center;

    public ChunkNeighbourhood(Chunk center)
    {
        AddChunk(center);
        Center = center;
    }

    public short GetBlock(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBlock(pos);
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBlock(pos, block, update);
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.UpdateBlock(pos);
    }

    public Chunk? GetChunk(Vector3D<int> pos, bool load = true)
    {
        return _neighbours[GetArrIndex(pos)];
    }

    public void AddChunk(Chunk chunk)
    {
        _neighbours[GetArrIndex(chunk.Position)] = chunk;
    }

    public void RemoveChunk(Vector3D<int> pos)
    {
        _neighbours[GetArrIndex(pos)] = null;
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return _neighbours.Where(c => c != null)!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & 3) | (((pos.Y & 3) | ((pos.Z & 3) << 2)) << 2);
    }
}