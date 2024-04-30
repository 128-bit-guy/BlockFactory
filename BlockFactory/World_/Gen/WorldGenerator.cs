using BlockFactory.Base;
using BlockFactory.World_.Light;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class WorldGenerator : IWorldGenerator
{
    private readonly BaseTerrainGenerator _baseTerrainGenerator;
    private readonly CaveGenerator _caveGenerator;
    private readonly WorldDecorator _decorator;

    public WorldGenerator(long seed)
    {
        Seed = seed;
        _baseTerrainGenerator = new BaseTerrainGenerator(this);
        _caveGenerator = new CaveGenerator(this);
        _decorator = new WorldDecorator(this);
    }

    public long Seed { get; }

    public void GenerateChunk(Chunk c)
    {
        c.Data = new ChunkData();
        var sky = _baseTerrainGenerator.GenerateBaseTerrain(c);
        if (sky)
        {
            for (var i = 0; i < Constants.ChunkSize; ++i)
            for (var j = 0; j < Constants.ChunkSize; ++j)
            for (var k = 0; k < Constants.ChunkSize; ++k)
            {
                var mi = Math.Min(Math.Min(i, j), k);
                var ma = Math.Max(i, k);
                var border = mi == 0 || ma == Constants.ChunkSize - 1;
                if (border)
                {
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
            _caveGenerator.GenerateCaves(c);
            for (var i = 0; i < Constants.ChunkSize; ++i)
            for (var j = 0; j < Constants.ChunkSize; ++j)
                c.Data.SetLightUpdateScheduled(new Vector3D<int>(i, Constants.ChunkSize - 1, j), true);
        }
    }

    public void DecorateChunk(Chunk c)
    {
        _decorator.DecorateChunk(c);
    }
}