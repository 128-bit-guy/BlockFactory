using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class World : IChunkStorage
{
    private readonly Dictionary<Vector3D<int>, Chunk> _chunks = new();
    public readonly WorldGenerator Generator = new WorldGenerator();

    public delegate void ChunkEventHandler(Chunk c);

    public event ChunkEventHandler ChunkReadyForUse = c => { };
    public event ChunkEventHandler ChunkNotReadyForUse = c => { };
    public event ChunkEventHandler ChunkReadyForTick = c => { };
    public event ChunkEventHandler ChunkNotReadyForTick = c => { };


    public short GetBlock(Vector3D<int> pos)
    {
        return GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.GetBlock(pos);
    }

    public void SetBlock(Vector3D<int> pos, short block)
    {
        GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.SetBlock(pos, block);
    }

    public Chunk? GetChunk(Vector3D<int> pos, bool load = true)
    {
        var res = _chunks.GetValueOrDefault(pos);
        if (res != null || !load) return res;
        var nc = new Chunk(pos);
        nc.Data = new ChunkData();
        Generator.GenerateChunk(nc);
        AddChunk(nc);
        return nc;
    }

    public void AddChunk(Chunk chunk)
    {
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

        OnChunkReadyForUse(chunk);
    }

    public void RemoveChunk(Vector3D<int> pos)
    {
        var c = _chunks[pos];
        OnChunkNotReadyForUse(c);
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
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return _chunks.Values;
    }

    private void OnChunkReadyForUse(Chunk c)
    {
        c.ReadyForUse = true;
        ChunkReadyForUse(c);
        ++c.ReadyForUseNeighbours;
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = c.Position + new Vector3D<int>(i, j, k);
            var oChunk = GetChunk(oPos, false);
            if (oChunk == null) continue;
            ++oChunk.ReadyForUseNeighbours;
            if (oChunk.ReadyForUseNeighbours == 27)
            {
                oChunk.ReadyForTick = true;
                ChunkReadyForTick(oChunk);
            }

            if (oChunk.ReadyForUse)
            {
                ++c.ReadyForUseNeighbours;
            }
        }

        if (c.ReadyForUseNeighbours == 27)
        {
            c.ReadyForTick = true;
            ChunkReadyForTick(c);
        }
    }
    
    private void OnChunkNotReadyForUse(Chunk c)
    {
        if (c.ReadyForUseNeighbours == 27)
        {
            ChunkNotReadyForTick(c);
            c.ReadyForTick = false;
        }

        --c.ReadyForUseNeighbours;

        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            var oPos = c.Position + new Vector3D<int>(i, j, k);
            var oChunk = GetChunk(oPos, false);
            if (oChunk == null) continue;
            if (oChunk.ReadyForUseNeighbours == 27)
            {
                ChunkNotReadyForTick(oChunk);
                oChunk.ReadyForTick = false;
            }

            --oChunk.ReadyForUseNeighbours;

            if (oChunk.ReadyForUse)
            {
                --c.ReadyForUseNeighbours;
            }
        }
        
        ChunkNotReadyForUse(c);
        c.ReadyForUse = false;
    }
}