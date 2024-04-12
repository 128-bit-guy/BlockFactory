using BlockFactory.Base;
using BlockFactory.Utils;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class WorldChunkStorage : IChunkStorage
{
    private readonly Dictionary<Vector3D<int>, Chunk> _chunks = new();

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
        return _chunks.ContainsKey(pos.ShiftRight(Constants.ChunkSizeLog2));
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
        return _chunks.GetValueOrDefault(pos);
    }

    public void AddChunk(Chunk chunk)
    {
        chunk.IsValid = true;
        _chunks.Add(chunk.Position, chunk);
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = chunk.Position + new Vector3D<int>(i, j, k);
            var oChunk = GetChunk(oPos, false);
            if (oChunk == null) continue;
            oChunk.Neighbourhood.AddChunk(chunk);
            chunk.Neighbourhood.AddChunk(oChunk);
        }
    }

    public void RemoveChunk(Vector3D<int> pos)
    {
        var c = _chunks[pos];
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = pos + new Vector3D<int>(i, j, k);
            var oChunk = GetChunk(oPos, false);
            if (oChunk == null) continue;
            oChunk.Neighbourhood.RemoveChunk(pos);
            c.Neighbourhood.RemoveChunk(oPos);
        }

        _chunks.Remove(pos);

        c.IsValid = false;
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return _chunks.Values;
    }
}