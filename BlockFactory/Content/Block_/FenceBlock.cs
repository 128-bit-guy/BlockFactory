using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using BlockFactory.World_.Light;

namespace BlockFactory.Content.Block_;

public class FenceBlock : Block
{
    public override int GetTexture(CubeFace face)
    {
        return 11;
    }

    public override bool CanLightEnter(CubeFace face, LightChannel channel)
    {
        return true;
    }

    public override bool CanLightLeave(CubeFace face, LightChannel channel)
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

    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    public virtual bool Connects(BlockPointer pointer, CubeFace face)
    {
        var oPointer = pointer + face.GetDelta();
        if (oPointer.GetBlockObj().IsFaceSolid(face.GetOpposite()))
        {
            return true;
        }

        return oPointer.GetBlockObj().Id == Id;
    }

    public override BlockLightType GetLightType()
    {
        return BlockLightType.Transparent;
    }
}