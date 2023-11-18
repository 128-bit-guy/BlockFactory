using BlockFactory.Block_;
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

            var center = pos + Vector3D<int>.UnitY * (height - 1);

            var radius = rng.Next(3, height - 1);
            
            for(var i = -radius; i <= radius; ++i)
            for(var j = -radius; j <= radius; ++j)
            for(var k = -radius; k <= radius; ++k)
            {
                var delta = new Vector3D<int>(i, j, k);
                if(delta.LengthSquared > radius * radius) continue;
                var leafPos = center + delta;
                if (world.GetBlock(leafPos) == 0)
                {
                    world.SetBlock(leafPos, Blocks.Leaves);
                }
            }
        }
    }
}