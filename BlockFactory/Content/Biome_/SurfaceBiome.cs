using BlockFactory.Content.Block_;
using BlockFactory.World_;
using BlockFactory.World_.Gen;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Content.Biome_;

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
        if (pointer.GetBlockObj().IsReplaceable() && (pointer - Vector3D<int>.UnitY).GetBlock() == Blocks.Grass.Id)
        {
            if (rng.Next(150) == 0)
            {
                TreeGenerator.Generate(pointer, rng);
            } else if (rng.Next(5) == 0)
            {
                pointer.SetBlock(Blocks.TallGrass);
            }
        }
    }
}