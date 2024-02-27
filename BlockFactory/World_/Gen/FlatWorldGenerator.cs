using BlockFactory.Base;
using BlockFactory.Biome_;
using BlockFactory.Block_;
using BlockFactory.Math_;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class FlatWorldGenerator : IWorldGenerator
{
    public void GenerateChunk(Chunk c)
    {
        c.Data = new ChunkData();
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var absPos = c.Position.ShiftLeft(Constants.ChunkSizeLog2) + new Vector3D<int>(i, j, k);
            if (absPos.Y == 0)
            {
                c.Data!.SetBlock(absPos, Blocks.Grass);
            }
            else if (absPos.Y == -1)
            {
                c.Data!.SetBlock(absPos, Blocks.Dirt);
            }
            else if (absPos.Y < -1)
            {
                c.Data!.SetBlock(absPos, 1);
                c.Data!.SetBiome(absPos, Biomes.Underground);
            }
            else
            {
                c.Data!.SetBiome(absPos, Biomes.Surface);
            }
        }

        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
            c.Data.SetLightUpdateScheduled(new Vector3D<int>(i, Constants.ChunkSize - 1, j), true);
    }

    public void DecorateChunk(Chunk c)
    {
    }
}