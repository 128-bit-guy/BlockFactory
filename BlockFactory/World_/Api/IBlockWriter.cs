using BlockFactory.Block_;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Api;

public interface IBlockWriter
{
    public void SetBlockState(Vector3i pos, BlockState state);
}