using BlockFactory.Base;
using BlockFactory.Content.Block_;
using BlockFactory.Utils;
using BlockFactory.Utils.Random_;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class CaveGenerator : WorldGenElement
{
    public CaveGenerator(WorldGenerator generator) : base(generator, -5564907199384973000)
    {
    }

    private void GenerateBranch(Random random, Vector3D<int> pos, float radius, Vector3D<float> direction,
        Chunk carvedChunk)
    {
        var initPos = pos;
        while (radius > 3f)
        {
            if ((pos - initPos).LengthSquared >= 16 * 16 * 4 * 4) break;
            SetSphere(pos, (int)MathF.Round(radius), 0, carvedChunk);
            pos += new Vector3D<int>((int)MathF.Round(direction.X * radius), (int)MathF.Round(direction.Y * radius),
                (int)MathF.Round(direction.Z * radius));
            var oDirection = RandomUtils.PointOnSphere(random);
            var nDirection = Vector3D.Normalize(2 * direction + oDirection);
            direction = nDirection;
            radius *= 0.98f;
        }
    }


    private void GenerateCave(Random random, Vector3D<int> center, Chunk carvedChunk)
    {
        var initialRadius = 4 + (float)random.NextDouble() * 4;
        var initialBranches = random.Next(3, 6);
        for (var i = 0; i < initialBranches; ++i)
            GenerateBranch(random, center, initialRadius, RandomUtils.PointOnSphere(random), carvedChunk);
    }

    private void GenerateCaveForOrigPos(Chunk c, Vector3D<int> origChunkPos)
    {
        var random = GetPosRandom(origChunkPos);
        Vector3D<int> rel;
        if (random.Next(120) != 0) return;
        var chunkOriginPos = origChunkPos.ShiftLeft(Constants.ChunkSizeLog2);
        rel.X = random.Next(Constants.ChunkSize);
        rel.Y = random.Next(Constants.ChunkSize);
        rel.Z = random.Next(Constants.ChunkSize);
        var center = chunkOriginPos + rel;
        GenerateCave(random, center, c);
    }

    public void GenerateCaves(Chunk c)
    {
        for (var i = -5; i <= 5; ++i)
        for (var j = -5; j <= 5; ++j)
        for (var k = -5; k <= 5; ++k)
        {
            var origChunkPos = c.Position + new Vector3D<int>(i, j, k);
            GenerateCaveForOrigPos(c, origChunkPos);
        }
    }

    private void SetSphere(Vector3D<int> center, int radius, short block, Chunk carvedChunk)
    {
        var mi = carvedChunk.Position.ShiftLeft(Constants.ChunkSizeLog2);
        var ma = mi + new Vector3D<int>(Constants.ChunkMask);
        for (var i = Math.Max(center.X - radius, mi.X); i <= Math.Min(center.X + radius, ma.X); ++i)
        for (var j = Math.Max(center.Y - radius, mi.Y); j <= Math.Min(center.Y + radius, ma.Y); ++j)
        for (var k = Math.Max(center.Z - radius, mi.Z); k <= Math.Min(center.Z + radius, ma.Z); ++k)
        {
            var pos = new Vector3D<int>(i, j, k);
            if ((pos - center).LengthSquared <= radius * radius)
                if (carvedChunk.Data!.GetBlock(pos) != Blocks.Water.Id)
                    carvedChunk.Data!.SetBlock(pos, block);
        }
    }
}