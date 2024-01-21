using BlockFactory.Base;
using BlockFactory.Biome_;
using BlockFactory.Math_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class BaseTerrainGenerator
{
    private readonly FastNoiseLite _noise = new();
    private readonly FastNoiseLite _mountainNoise = new();
    private readonly FastNoiseLite _biomeHeightNoise = new();

    public BaseTerrainGenerator()
    {
        _mountainNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
    }

    public void GenerateBaseTerrain(Chunk c)
    {
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X;
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z;
            var val = _noise.GetNoise(x * 3.0d, z * 3.0d) +
                      Math.Max(0, _mountainNoise.GetNoise(x / 10.0d, z / 10.0d) - 2.0f / 7) * 70;
            var biomeVal = _biomeHeightNoise.GetNoise(x * 3.0d, z * 3.0d);
            for (var j = 0; j < Constants.ChunkSize; ++j)
            {
                var y = j + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Y;
                bool underground = false;
                if (val >= y / 2.0f)
                {
                    c.Data!.SetBlock(new Vector3D<int>(x, y, z), 1);
                    if (val >= (y + 5) / 2.0f)
                    {
                        c.Data!.SetBiome(new Vector3D<int>(x, y, z), 1);
                        underground = true;
                    }
                }

                if (underground) continue;
                switch (y + biomeVal * 3)
                {
                    case <= 20:
                        continue;
                    default:
                        c.Data!.SetBiome(new Vector3D<int>(x, y, z), Biomes.BigHeight);
                        break;
                }
            }
        }
    }
}