using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Util;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;
using SharpNoise.Modules;

namespace BlockFactory.World_.Gen;

public class WorldGenerator
{
    public readonly int Seed;
    public Perlin Perlin;

    public WorldGenerator(int seed)
    {
        Seed = seed;
        Perlin = new Perlin
        {
            Seed = unchecked(1798783699 * seed + 1675735027),
            Frequency = 2f,
            Lacunarity = 2f,
            Persistence = 2f,
            OctaveCount = 3
        };
    }

    private Random GetChunkRandom(Vector3i pos, int a, int b, int c, int d)
    {
        return new LinearCongruentialRandom(unchecked(pos.X * a + pos.Y * b + pos.Z * c + d * Seed));
    }

    public void GenerateBaseSurface(Chunk chunk)
    {
        var random = GetChunkRandom(chunk.Pos, 1401634909, 1527589979, 1057394087, 1642081541);
        for (var x = 0; x < Constants.ChunkSize; ++x)
        {
            var absX = chunk.GetBegin().X + x;
            for (var z = 0; z < Constants.ChunkSize; ++z)
            {
                var absZ = chunk.GetBegin().Z + z;
                var noise = Perlin.GetValue(absX / 100f, 0, absZ / 100f);
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

    public void Decorate(Chunk c)
    {
        PlaceTopSoil(c);
        AddPlants(c);
    }

    private void PlaceTopSoil(Chunk chunk)
    {
        var random = GetChunkRandom(chunk.Pos, 1454274037, 1016482381, 1497360727, 1925636137);
        foreach (var a1 in chunk.GetInclusiveBox().InclusiveEnumerable())
        {
            var a = a1 - Vector3i.UnitY;
            if ((chunk.Neighbourhood.GetBlockState(a).Block == Blocks.Stone ||
                 chunk.Neighbourhood.GetBlockState(a).Block == Blocks.Dirt) &&
                chunk.Neighbourhood.GetBlockState(a + new Vector3i(0, 1, 0)).Block == Blocks.Air)
            {
                var oPos = a;
                while (true)
                {
                    if (a.Y - oPos.Y >= 5) break;
                    if (chunk.Neighbourhood.GetBlockState(oPos).Block == Blocks.Stone ||
                        chunk.Neighbourhood.GetBlockState(oPos).Block == Blocks.Dirt)
                    {
                        if (oPos == a)
                            chunk.Neighbourhood.SetBlockState(oPos, new BlockState(Blocks.Grass,
                                RandomRotations.KeepingUp(random)));
                        else
                            chunk.Neighbourhood.SetBlockState(oPos, new BlockState(Blocks.Dirt,
                                RandomRotations.Any(random)));
                        if (random.Next(3) <= 1)
                            oPos -= new Vector3i(0, 1, 0);
                        else
                            break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    private void AddPlants(Chunk chunk)
    {
        var random = GetChunkRandom(chunk.Pos, 1514407261, 1177000723, 1707795989, 1133321689);
        foreach (var a in chunk.GetInclusiveBox().InclusiveEnumerable())
        {
            var downState = chunk.Neighbourhood.GetBlockState(a - Vector3i.UnitY);
            if (downState.Block == Blocks.Grass && downState.Rotation * Direction.Up == Direction.Up)
                if (random.Next(100) == 0)
                {
                    var height = random.Next(5, 8);
                    for (var i = 0; i < height; ++i)
                    {
                        var state = new BlockState(Blocks.Log, RandomRotations.KeepingY(random));
                        chunk.Neighbourhood.SetBlockState(a + Vector3i.UnitY * i, state);
                    }

                    var logTop = a + Vector3i.UnitY * (height - 1);
                    foreach (var pos in WorldEnumerators.GetSphereEnumerator(logTop,
                                 random.Next(3, Math.Min(height - 1, 7))))
                        if (chunk.Neighbourhood.GetBlockState(pos).Block == Blocks.Air)
                            chunk.Neighbourhood.SetBlockState(pos, new BlockState(Blocks.Leaves,
                                RandomRotations.Any(random)));
                }
        }
    }
}