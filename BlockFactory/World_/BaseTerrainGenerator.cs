using BlockFactory.Base;
using BlockFactory.Math_;
using SharpNoise.Modules;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class BaseTerrainGenerator
{
    private readonly Perlin _perlin = new();

    public void GenerateBaseTerrain(Chunk c)
    {
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X;
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z;
            var val = _perlin.GetValue(x / 10.0f, z / 10.0f, 0);
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