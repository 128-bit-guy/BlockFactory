using BlockFactory.Base;
using BlockFactory.Block_;
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
        _oreGenerators.Add(new OreGenerator(Blocks.IronOre, Blocks.Stone, 3, 9, 15, 1/2000f));
        _oreGenerators.Add(new OreGenerator(Blocks.CopperOre, Blocks.Stone, 3, 9, 15, 1/2000f));
        _oreGenerators.Add(new OreGenerator(Blocks.TinOre, Blocks.Stone, 3, 9, 15, 1/2000f));
        _oreGenerators.Add(new OreGenerator(Blocks.DiamondOre, Blocks.Stone, 3, 9, 15, 1/20000f));
        _oreGenerators.Add(new OreGenerator(Blocks.CoalOre, Blocks.Stone, 7, 18, 15, 1/2000f));
        
    }

    public void DecorateChunk(Chunk c)
    {
        var rng = GetChunkRandom(c);
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        {
            var x = i + c.Position.ShiftLeft(Constants.ChunkSizeLog2).X;
            var y = j + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Y;
            var z = k + c.Position.ShiftLeft(Constants.ChunkSizeLog2).Z;
            if (c.Neighbourhood.GetBlock(new Vector3D<int>(x, y - 1, z)) == 1 &&
                c.Neighbourhood.GetBlock(new Vector3D<int>(x, y, z)) == 0)
            {
                c.Neighbourhood.GetBiomeObj(new Vector3D<int>(x, y, z))
                    .SetTopSoil(c.Neighbourhood, new Vector3D<int>(x, y - 1, z));
            }

            c.Neighbourhood.GetBiomeObj(new Vector3D<int>(x, y, z))
                .Decorate(c.Neighbourhood, new Vector3D<int>(x, y, z), rng);
            foreach (var generator in _oreGenerators)
            {
                generator.Generate(c.Neighbourhood, new Vector3D<int>(x, y, z), rng);
            }
        }
    }
}