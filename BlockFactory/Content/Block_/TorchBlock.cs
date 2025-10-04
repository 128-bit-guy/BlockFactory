using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Physics;
using BlockFactory.World_;
using BlockFactory.World_.Light;

namespace BlockFactory.Content.Block_;

public class TorchBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return face == CubeFace.Top ? 13 : 12;
    }

    public override byte GetEmittedLight()
    {
        return 15;
    }

    public override bool CanLightEnter(CubeFace face, LightChannel channel)
    {
        return true;
    }

    public override bool CanLightLeave(CubeFace face, LightChannel channel)
    {
        return true;
    }

    public override bool HasAo()
    {
        return false;
    }

    public override bool IsFaceSolid(CubeFace face)
    {
        return false;
    }

    public override void AddBlockBoxes(ConstBlockPointer pointer, BoxConsumer.BoxConsumerFunc consumer, BlockBoxType type)
    {
        if(type == BlockBoxType.Collision) return;
        base.AddBlockBoxes(pointer, consumer, type);
    }
}