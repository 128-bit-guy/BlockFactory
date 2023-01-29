using OpenTK.Mathematics;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Side_;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;

namespace BlockFactory.Client.World_;

[ExclusiveTo(Side.Client)]
public class EmptyBlockReader : IBlockReader
{
    public BlockState GetBlockState(Vector3i pos)
    {
        return new BlockState(Blocks.Air, CubeRotation.Rotations[0]);
    }
}