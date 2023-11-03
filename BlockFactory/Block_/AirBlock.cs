using BlockFactory.Base;
using BlockFactory.CubeMath;

namespace BlockFactory.Block_;

public class AirBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    public override bool CanLightEnter(CubeFace face)
    {
        return true;
    }
}