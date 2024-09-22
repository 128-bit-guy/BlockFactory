using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Content.Entity_;
using BlockFactory.Utils;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class ChunkNeighbourhood : IChunkWorld
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

    public void ScheduleLightUpdate(Vector3D<int> pos)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.ScheduleLightUpdate(pos);
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

    public bool IsBlockLoaded(Vector3D<int> pos)
    {
        var diff = pos.ShiftRight(Constants.ChunkSizeLog2) - Center.Position;
        for (var i = 0; i < 3; ++i)
        {
            if (Math.Abs(diff[i]) > 1) return false;
        }

        return true;
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
        var c = _neighbours[GetArrIndex(pos)];
        return c ?? Center.World.GetChunk(pos, load);
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

    public void UpdateLight(Vector3D<int> pos)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.ChunkUpdateInfo.UpdateLight(pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & 3) | (((pos.Y & 3) | ((pos.Z & 3) << 2)) << 2);
    }

    public Entity? GetEntity(Guid guid)
    {
        foreach (var chunk in _neighbours)
        {
            var e = chunk?.GetEntity(guid);
            if (e != null)
            {
                return e;
            }
        }

        return null;
    }

    public IEnumerable<Entity> GetEntities(Box3D<double> box)
    {
        var res = new List<Entity>();
        foreach (var chunk in GetLoadedChunks())
        {
            res.AddRange(chunk.GetEntities(box));
        }

        return res;
    }

    public void AddEntity(Entity entity)
    {
        throw new NotImplementedException();
    }

    public void RemoveEntity(Entity entity)
    {
        throw new NotImplementedException();
    }
}