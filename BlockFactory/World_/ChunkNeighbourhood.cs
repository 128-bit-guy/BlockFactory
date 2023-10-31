using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Math_;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
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

    public void UpdateBlock(Vector3D<int> pos)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.UpdateBlock(pos);
    }

    public short GetBlock(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBlock(pos);
    }

    public byte GetBiome(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBiome(pos);
    }

    public byte GetLight(Vector3D<int> pos, LightChannel channel)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetLight(pos, channel);
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBlock(pos, block, update);
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBiome(pos, biome);
    }

    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetLight(pos, channel, light);
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