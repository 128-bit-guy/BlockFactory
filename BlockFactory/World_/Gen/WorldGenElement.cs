using BlockFactory.Utils.Random_;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class WorldGenElement
{
    private const long HalfMask = uint.MaxValue;
    private readonly long _uniqueNumber;
    public readonly WorldGenerator Generator;

    public WorldGenElement(WorldGenerator generator, long uniqueNumber)
    {
        Generator = generator;
        _uniqueNumber = uniqueNumber;
    }

    public long GetChunkSeed(Vector3D<int> pos)
    {
        var resPart1 =
            (((((pos.X + pos.Y * 1000000007) & HalfMask) +
               ((((pos.Z * 1000000007) & HalfMask) * 1000000009) & HalfMask)) & HalfMask) +
             (Generator.Seed ^ _uniqueNumber) % (HalfMask + 2)) & HalfMask;
        var resPart2 = (_uniqueNumber >> 32) ^ (Generator.Seed >> 32);
        var res = resPart1 | (resPart2 << 32);
        return res;
    }

    public long GetChunkSeed(Chunk c)
    {
        return GetChunkSeed(c.Position);
    }

    public LinearCongruentialRandom GetChunkRandom(Vector3D<int> c)
    {
        var rng = LinearCongruentialRandom.ThreadLocalInstance;
        rng.SetSeed(GetChunkSeed(c));
        return rng;
    }

    public LinearCongruentialRandom GetChunkRandom(Chunk c)
    {
        var rng = LinearCongruentialRandom.ThreadLocalInstance;
        rng.SetSeed(GetChunkSeed(c));
        return rng;
    }
}