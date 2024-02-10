using BlockFactory.Block_;
using BlockFactory.World_;
using BlockFactory.World_.Gen;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Biome_;

public class SurfaceBiome : Biome
{
    public override void SetTopSoil(IBlockStorage world, Vector3D<int> pos)
    {
        world.SetBlock(pos, Blocks.Grass);
        if (world.GetBlockObj(pos - Vector3D<int>.UnitY).GetWorldGenBase() == Blocks.Stone)
            world.SetBlock(pos - Vector3D<int>.UnitY, Blocks.Dirt);
    }

    public override void Decorate(BlockPointer pointer, Random rng)
    {
        if (pointer.GetBlock() == 0 && (pointer - Vector3D<int>.UnitY).GetBlock() == Blocks.Grass.Id &&
            rng.Next(150) == 0)
        {
            TreeGenerator.Generate(pointer, rng);
        }
    }
}