using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Block_;

public class DirtBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return 2;
    }

    public override void RandomUpdateBlock(BlockPointer pointer)
    {
        if ((pointer + Vector3D<int>.UnitY).GetBlockObj().IsFaceSolid(CubeFace.Bottom)) return;
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if((pointer + new Vector3D<int>(i, j, k)).GetBlock() != Blocks.Grass.Id) continue;
            pointer.SetBlock(Blocks.Grass);
            goto EndLoop;
        }

        EndLoop: ;
    }
}