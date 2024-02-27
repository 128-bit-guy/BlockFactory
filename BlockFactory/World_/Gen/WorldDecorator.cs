using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Math_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class WorldDecorator : WorldGenElement
{
    private readonly List<OreGenerator> _oreGenerators;

    public WorldDecorator(WorldGenerator generator) : base(generator, -1320159947507007282)
    {
        _oreGenerators = new List<OreGenerator>();
        _oreGenerators.Add(new OreGenerator(Blocks.IronOre, Blocks.Stone, 3, 9, 15, 1 / 2000f));
        _oreGenerators.Add(new OreGenerator(Blocks.CopperOre, Blocks.Stone, 3, 9, 15, 1 / 2000f));
        _oreGenerators.Add(new OreGenerator(Blocks.TinOre, Blocks.Stone, 3, 9, 15, 1 / 2000f));
        _oreGenerators.Add(new OreGenerator(Blocks.DiamondOre, Blocks.Stone, 3, 9, 15, 1 / 20000f));
        _oreGenerators.Add(new OreGenerator(Blocks.CoalOre, Blocks.Stone, 12, 28, 15, 1 / 2000f));
    }

    private void PlaceTopSoil(Chunk c)
    {
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X + (Constants.ChunkSize >> 1);
            var y = j + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Y + (Constants.ChunkSize >> 1);
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z + (Constants.ChunkSize >> 1);
            if (c.Neighbourhood.GetBlockObj(new Vector3D<int>(x, y - 1, z)).GetWorldGenBase() == Blocks.Stone &&
                !c.Neighbourhood.GetBlockObj(new Vector3D<int>(x, y, z)).IsFaceSolid(CubeFace.Bottom))
                c.Neighbourhood.GetBiomeObj(new Vector3D<int>(x, y, z))
                    .SetTopSoil(c.Neighbourhood, new Vector3D<int>(x, y - 1, z));
        }
    }

    private void EnsureTopSoilFullyPlaced(Chunk c)
    {
        for (var i = -1; i <= 0; ++i)
        for (var j = -1; j <= 0; ++j)
        for (var k = -1; k <= 0; ++k)
        {
            var n = c.Neighbourhood.GetChunk(c.Position + new Vector3D<int>(i, j, k))!;
            if (n.Data!.TopSoilPlaced) continue;
            n.Data!.TopSoilPlaced = true;
            PlaceTopSoil(n);
        }
    }

    public void DecorateChunk(Chunk c)
    {
        var rng = GetChunkRandom(c);
        EnsureTopSoilFullyPlaced(c);
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X;
            var y = j + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Y;
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z;

            c.Neighbourhood.GetBiomeObj(new Vector3D<int>(x, y, z))
                .Decorate(new BlockPointer(c.Neighbourhood, new Vector3D<int>(x, y, z)), rng);
            foreach (var generator in _oreGenerators)
                generator.Generate(c.Neighbourhood, new Vector3D<int>(x, y, z), rng);
        }
    }
}