using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Init;
using BlockFactory.Util;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;
using SharpNoise.Modules;

namespace BlockFactory.World_.Gen;

public class BaseSurfaceGenerator
{
    public readonly WorldGenerator WorldGenerator;
    public Perlin Perlin;

    public BaseSurfaceGenerator(WorldGenerator worldGenerator)
    {
        WorldGenerator = worldGenerator;
        Perlin = new Perlin
        {
            Seed = unchecked(1798783699 * worldGenerator.Seed + 1675735027),
            Frequency = 2f,
            Lacunarity = 2f,
            Persistence = 2f,
            OctaveCount = 3
        };
    }

    private void AddFeatures(Vector2i featureChunkPos, Vector2i affectedChunkPos, double[,] affectedChunkHeights)
    {
        var random = WorldGenerator.GetChunkRandom(
            new Vector3i(featureChunkPos.X, 0, featureChunkPos.Y),
            1927163633, 0, 1272879857, 1501771811
        );
        if (random.Next(50) != 0) return;
        var height = random.Next(50, 80);
        var relX = random.Next(Constants.ChunkSize);
        var relY = random.Next(Constants.ChunkSize);
        var relPos1 = new Vector2i(relX, relY);
        var relToChunk = affectedChunkPos * Constants.ChunkSize -
                         (relPos1 + featureChunkPos * Constants.ChunkSize);
        var modifier = random.NextDouble() + 1;
        foreach (var relPos2 in new Box2i(new Vector2i(0), new Vector2i(Constants.ChunkSize - 1))
                     .InclusiveEnumerable())
        {
            var delta = relToChunk + relPos2;
            double curVal = height - delta.EuclideanLength;
            affectedChunkHeights[relPos2.X, relPos2.Y] =
                Math.Max(affectedChunkHeights[relPos2.X, relPos2.Y], curVal * modifier);
        }
    }

    public void GenerateBaseSurface(Chunk chunk)
    {
        var heights = new double[Constants.ChunkSize, Constants.ChunkSize];
        foreach (var affectedChunkDelta in new Box2i(new Vector2i(-5, -5), new Vector2i(5, 5))
                     .InclusiveEnumerable())
        {
            AddFeatures(chunk.Pos.Xz + affectedChunkDelta, chunk.Pos.Xz, heights);
        }

        var random = WorldGenerator.GetChunkRandom(chunk.Pos, 1401634909, 1527589979, 1057394087, 1642081541);
        for (var x = 0; x < Constants.ChunkSize; ++x)
        {
            var absX = chunk.GetBegin().X + x;
            for (var z = 0; z < Constants.ChunkSize; ++z)
            {
                var absZ = chunk.GetBegin().Z + z;
                var noise = heights[x, z] + Perlin.GetValue(absX / 100f, 0, absZ / 100f);
                for (var y = 0; y < Constants.ChunkSize; ++y)
                {
                    var absY = chunk.GetBegin().Y + y;
                    if (noise >= absY)
                        chunk.Neighbourhood.SetBlockState((absX, absY, absZ), new BlockState(Blocks.Stone,
                            RandomRotations.Any(random)));
                }
            }
        }
    }
}