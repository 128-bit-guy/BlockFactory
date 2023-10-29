using BlockFactory.Registry_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Biome_;

public class Biome : IRegistryEntry
{
    public int Id { get; set; }

    public virtual void SetTopSoil(IBlockStorage world, Vector3D<int> pos)
    {
        
    }

    public virtual void Decorate(IBlockStorage world, Vector3D<int> pos, Random rng)
    {
        
    }
}