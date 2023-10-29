using BlockFactory.Block_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Biome_;

public class SurfaceBiome : Biome
{
    public override void SetTopSoil(IBlockStorage world, Vector3D<int> pos)
    {
        world.SetBlock(pos, Blocks.Grass);
        if (world.GetBlock(pos - Vector3D<int>.UnitY) == 1)
            world.SetBlock(pos - Vector3D<int>.UnitY, Blocks.Dirt);
    }

    public override void Decorate(IBlockStorage world, Vector3D<int> pos, Random rng)
    {
        if (world.GetBlock(pos) == 0 && world.GetBlock(pos - Vector3D<int>.UnitY) == Blocks.Grass.Id &&
            rng.Next(150) == 0)
        {
            var height = rng.Next(5, 8);
            for (var i = 0; i < height; ++i)
            {
                world.SetBlock(pos + Vector3D<int>.UnitY * i, Blocks.Log);
            }
        }
    }
}