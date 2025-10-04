using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Physics;
using BlockFactory.World_;
using BlockFactory.World_.Light;

namespace BlockFactory.Content.Block_;

public class WaterBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return 9;
    }

    public override bool IsFaceSolid(CubeFace face)
    {
        return false;
    }

    [ExclusiveTo(Side.Client)]
    public override bool HasAo()
    {
        return false;
    }

    public override bool CanLightEnter(CubeFace face, LightChannel channel)
    {
        return channel != LightChannel.DirectSky;
    }

    public override void UpdateBlock(BlockPointer pointer)
    {
        base.UpdateBlock(pointer);
        foreach (var face in CubeFaceUtils.Values())
        {
            if (face == CubeFace.Top) continue;
            var oPointer = pointer + face.GetDelta();
            if (oPointer.GetBlock() == 0) oPointer.SetBlock(this);
        }
    }

    public override bool IsReplaceable()
    {
        return true;
    }

    public override void AddBlockBoxes(ConstBlockPointer pointer, BoxConsumer.BoxConsumerFunc consumer, BlockBoxType type)
    {
    }
}