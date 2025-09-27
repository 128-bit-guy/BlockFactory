using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_.Light;

namespace BlockFactory.Content.Block_;

public class AirBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    public override bool CanLightEnter(CubeFace face, LightChannel channel)
    {
        return true;
    }

    [ExclusiveTo(Side.Client)]
    public override bool HasAo()
    {
        return false;
    }

    public override bool IsFaceSolid(CubeFace face)
    {
        return false;
    }

    public override bool HasCollision()
    {
        return false;
    }

    public override bool IsReplaceable()
    {
        return true;
    }

    public override BlockLightType GetLightType()
    {
        return BlockLightType.Transparent;
    }
}