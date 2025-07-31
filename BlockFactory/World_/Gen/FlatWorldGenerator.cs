using System.Collections;
using BlockFactory.Base;
using BlockFactory.Content.Biome_;
using BlockFactory.Content.Block_;
using BlockFactory.Utils;
using BlockFactory.Utils.Random_;
using BlockFactory.World_.Gen.Mineshaft;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class FlatWorldGenerator : IWorldGenerator
{
    private readonly MineshaftGenerator _mineshaftGenerator;

    public FlatWorldGenerator(long seed)
    {
        Seed = seed;
        _mineshaftGenerator = new MineshaftGenerator(this, 1937742181);
    }

    public long Seed { get; }

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

        var sky = c.Position.Y > 0;

        if (sky)
        {
            var cnt = 0;
            for (var i = 0; i < Constants.ChunkSize; ++i)
            for (var j = 0; j < Constants.ChunkSize; ++j)
            for (var k = 0; k < Constants.ChunkSize; ++k)
            {
                var mi = Math.Min(Math.Min(i, j), k);
                var ma = Math.Max(i, k);
                var border = mi == 0 || ma == Constants.ChunkSize - 1;
                if (border)
                {
                    ++cnt;
                    c.Data.SetLightUpdateScheduled(new Vector3D<int>(i, j, k), true);
                }
                else
                {
                    c.Data.SetLight(new Vector3D<int>(i, j, k), LightChannel.DirectSky, 15);
                    c.Data.SetLight(new Vector3D<int>(i, j, k), LightChannel.Sky, 15);
                }
            }
        }
        else
        {
            for (var i = 0; i < Constants.ChunkSize; ++i)
            for (var j = 0; j < Constants.ChunkSize; ++j)
                c.Data.SetLightUpdateScheduled(new Vector3D<int>(i, Constants.ChunkSize - 1, j), true);
        }
    }

    public void DecorateChunk(Chunk c)
    {
        _mineshaftGenerator.Generate(c, new LinearCongruentialRandom(0));
    }

    public string GetName()
    {
        return "Flat";
    }
}