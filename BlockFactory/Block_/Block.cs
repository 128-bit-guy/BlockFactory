using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Registry_;

namespace BlockFactory.Block_;

public class Block : IRegistryEntry
{
    public int Id { get; set; }

    [ExclusiveTo(Side.Client)]
    public virtual bool BlockRendering(CubeFace face)
    {
        return true;
    }

    [ExclusiveTo(Side.Client)]
    public virtual int GetTexture(CubeFace face)
    {
        return 0;
    }
}