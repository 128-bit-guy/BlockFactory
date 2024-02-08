using BlockFactory.Base;
using BlockFactory.Biome_;
using BlockFactory.Block_;
using BlockFactory.Math_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class BaseTerrainGenerator
{
    private readonly FastNoiseLite _noise = new();
    private readonly FastNoiseLite _mountainNoise = new();
    private readonly FastNoiseLite _biomeHeightNoise = new();
    private readonly FastNoiseLite _oceanNoise = new();
    [ThreadStatic] private static int[,]? _oceanMap;
    private const int OceanSmoothRadius = 5;

    public BaseTerrainGenerator()
    {
        _mountainNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
    }

    private static void InitThreadStatics()
    {
        _oceanMap ??= new int[Constants.ChunkSize + 2 * OceanSmoothRadius, Constants.ChunkSize + 2 * OceanSmoothRadius];
    }

    public void GenerateBaseTerrain(Chunk c)
    {
        InitThreadStatics();
        var cBegin = c.Position.ShiftLeft(Constants.ChunkSizeLog2);
        for (var i = -OceanSmoothRadius; i < Constants.ChunkSize + OceanSmoothRadius; ++i)
        for (var j = -OceanSmoothRadius; j < Constants.ChunkSize + OceanSmoothRadius; ++j)
        {
            var x = i + cBegin.X;
            var z = j + cBegin.Z;
            var val = _oceanNoise.GetNoise(x / 10.0d, z / 10.0d);
            _oceanMap![i + OceanSmoothRadius, j + OceanSmoothRadius] = val < 0.3 ? val < 0.2f? val < 0.1f? val < -0.5f ? 4 : 3 : 2 : 1 : 0;
        }

        Span<float> heights = stackalloc float[5];

        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X;
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z;
            heights[0] = Math.Max(0, _mountainNoise.GetNoise(x / 10.0d, z / 10.0d) - 2.0f / 7) * 140 + 2;
            heights[1] = 2;
            heights[2] = -5;
            heights[3] = -32;
            heights[4] = -64;
            var smoothHeight = 0.0f;
            for (var l = -OceanSmoothRadius; l <= OceanSmoothRadius; ++l)
            for (var m = -OceanSmoothRadius; m <= OceanSmoothRadius; ++m)
            {
                smoothHeight += heights[_oceanMap![i + l + OceanSmoothRadius, k + m + OceanSmoothRadius]];
            }
            smoothHeight /= ((2 * OceanSmoothRadius + 1) * (2 * OceanSmoothRadius + 1));
            var val = smoothHeight + _noise.GetNoise(x * 3.0d, z * 3.0d) * 2;
            var biomeVal = _biomeHeightNoise.GetNoise(x * 3.0d, z * 3.0d);
            for (var j = 0; j < Constants.ChunkSize; ++j)
            {
                var y = j + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Y;
                bool underground = false;
                if (val >= y)
                {
                    c.Data!.SetBlock(new Vector3D<int>(x, y, z), 1);
                    if (val >= y + 5)
                    {
                        c.Data!.SetBiome(new Vector3D<int>(x, y, z), 1);
                        underground = true;
                    } else if (_oceanMap![i + OceanSmoothRadius, k + OceanSmoothRadius] is >= 1 and <= 2)
                    {
                        c.Data!.SetBiome(new Vector3D<int>(x, y, z), Biomes.Beach);
                    } else if (_oceanMap[i + OceanSmoothRadius, k + OceanSmoothRadius] > 2)
                    {
                        c.Data!.SetBiome(new Vector3D<int>(x, y, z), Biomes.Ocean);
                    }
                }
                else if (y < 0)
                {
                    c.Data!.SetBlock(new Vector3D<int>(x, y, z), Blocks.Water);
                }

                if (underground) continue;
                if (_oceanMap![i + OceanSmoothRadius, k + OceanSmoothRadius] is >= 1 and <= 2)
                {
                    c.Data!.SetBiome(new Vector3D<int>(x, y, z), Biomes.Beach);
                } else if (_oceanMap[i + OceanSmoothRadius, k + OceanSmoothRadius] > 2)
                {
                    c.Data!.SetBiome(new Vector3D<int>(x, y, z), Biomes.Ocean);
                }
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