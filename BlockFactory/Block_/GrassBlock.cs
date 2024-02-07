using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Block_;

public class GrassBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return face switch
        {
            CubeFace.Top => 3,
            CubeFace.Bottom => 2,
            _ => 4
        };
    }

    public override void UpdateBlock(BlockPointer pointer)
    {
        if ((pointer + Vector3D<int>.UnitY).GetBlockObj().IsFaceSolid(CubeFace.Bottom)) pointer.SetBlock(Blocks.Dirt);
    }
}