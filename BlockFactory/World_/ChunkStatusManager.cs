using Silk.NET.Maths;

namespace BlockFactory.World_;

public class ChunkStatusManager
{
    public delegate void ChunkEventHandler(Chunk c);

    public readonly World World;

    public ChunkStatusManager(World world)
    {
        World = world;
    }

    public event ChunkEventHandler ChunkReadyForUse = c => { };
    public event ChunkEventHandler ChunkNotReadyForUse = c => { };
    public event ChunkEventHandler ChunkReadyForTick = c => { };
    public event ChunkEventHandler ChunkNotReadyForTick = c => { };

    public void OnChunkReadyForUse(Chunk c)
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
            var oChunk = World.GetChunk(oPos, false);
            if (oChunk == null) continue;
            ++oChunk.ReadyForUseNeighbours;
            if (oChunk.ReadyForUseNeighbours == 27)
            {
                oChunk.ReadyForTick = true;
                ChunkReadyForTick(oChunk);
            }

            if (oChunk.ReadyForUse) ++c.ReadyForUseNeighbours;
        }

        if (c.ReadyForUseNeighbours == 27)
        {
            c.ReadyForTick = true;
            ChunkReadyForTick(c);
        }
    }

    public void OnChunkNotReadyForUse(Chunk c)
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
            var oChunk = World.GetChunk(oPos, false);
            if (oChunk == null) continue;
            if (oChunk.ReadyForUseNeighbours == 27)
            {
                ChunkNotReadyForTick(oChunk);
                oChunk.ReadyForTick = false;
            }

            --oChunk.ReadyForUseNeighbours;

            if (oChunk.ReadyForUse) --c.ReadyForUseNeighbours;
        }

        ChunkNotReadyForUse(c);
        c.ReadyForUse = false;
    }
}