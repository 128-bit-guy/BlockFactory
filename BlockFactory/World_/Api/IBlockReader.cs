using BlockFactory.Block_;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Api;

public interface IBlockReader
{
    public BlockState GetBlockState(Vector3i pos);
}