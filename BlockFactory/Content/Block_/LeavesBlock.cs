using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_.Light;

namespace BlockFactory.Content.Block_;

public class LeavesBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return 8;
    }

    public override bool CanLightEnter(CubeFace face, LightChannel channel)
    {
        return channel != LightChannel.DirectSky;
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
}