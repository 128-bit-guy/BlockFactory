using BlockFactory.Base;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class BaseTerrainGenerator
{
    private readonly FastNoiseLite _noise = new FastNoiseLite();
    private readonly FastNoiseLite _mountainNoise = new FastNoiseLite();

    public void GenerateBaseTerrain(Chunk c)
    {
        _mountainNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X;
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z;
            var val = _noise.GetNoise(x * 3.0d, z * 3.0d) + Math.Max(0, _mountainNoise.GetNoise(x / 10.0d, z / 10.0d)) * 50;
            for (var j = 0; j < Constants.ChunkSize; ++j)
            {
                var y = j + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Y;
                if (val >= y / 2.0f)
                {
                    c.Data!.SetBlock(new Vector3D<int>(x, y, z), 1);
                    c.Data!.SetBiome(new Vector3D<int>(x, y, z), 1);
                }
            }
        }
    }
}