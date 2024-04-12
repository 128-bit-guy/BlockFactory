using BlockFactory.Registry_;
using BlockFactory.World_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Content.Biome_;

public class Biome : IRegistryEntry
{
    public int Id { get; set; }

    public virtual void SetTopSoil(IBlockStorage world, Vector3D<int> pos)
    {
    }

    public virtual void Decorate(BlockPointer pointer, Random rng)
    {
    }
}