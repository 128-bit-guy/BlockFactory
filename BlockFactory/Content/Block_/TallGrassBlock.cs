using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Content.Block_;

public class TallGrassBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return 14;
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

    public override bool HasCollision()
    {
        return false;
    }

    public override void UpdateBlock(BlockPointer pointer)
    {
        var down = pointer - Vector3D<int>.UnitY;
        if (down.GetBlock() != Blocks.Grass.Id && down.GetBlock() != Blocks.Dirt.Id)
        {
            pointer.SetBlock(0);
        }
    }

    public override bool IsReplaceable()
    {
        return true;
    }
}