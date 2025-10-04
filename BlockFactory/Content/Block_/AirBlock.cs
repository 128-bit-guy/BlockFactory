using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Physics;
using BlockFactory.World_;
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

    public override bool IsReplaceable()
    {
        return true;
    }

    public override void AddBlockBoxes(ConstBlockPointer pointer, BoxConsumer.BoxConsumerFunc consumer, BlockBoxType type)
    {
    }
}