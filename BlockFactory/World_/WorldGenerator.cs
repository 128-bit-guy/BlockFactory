using BlockFactory.Base;
using BlockFactory.Math_;
using SharpNoise.Modules;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class WorldGenerator
{
    private readonly Perlin _perlin = new();

    public void GenerateChunk(Chunk c)
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
                if (val >= y / 2.0f) c.SetBlock(new Vector3D<int>(x, y, z), 1, false);
            }
        }
    }

    public void DecorateChunk(Chunk c)
    {
        
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X;
            var y = j + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Y;
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z;
            if (c.Neighbourhood.GetBlock(new Vector3D<int>(x, y, z)) == 1 &&
                c.Neighbourhood.GetBlock(new Vector3D<int>(x, y + 1, z)) == 0)
            {
                c.Neighbourhood.SetBlock(new Vector3D<int>(x, y, z), 4);
                if (c.Neighbourhood.GetBlock(new Vector3D<int>(x, y - 1, z)) == 1)
                {
                    c.Neighbourhood.SetBlock(new Vector3D<int>(x, y - 1, z), 3);
                }
            }
        }
    }
}