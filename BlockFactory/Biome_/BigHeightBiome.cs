using BlockFactory.Block_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Biome_;

public class BigHeightBiome : Biome
{
    public override void SetTopSoil(IBlockStorage world, Vector3D<int> pos)
    {
        world.SetBlock(pos, Blocks.Grass);
        if (world.GetBlockObj(pos - Vector3D<int>.UnitY).GetWorldGenBase() == Blocks.Stone)
            world.SetBlock(pos - Vector3D<int>.UnitY, Blocks.Dirt);
    }
}