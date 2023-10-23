using BlockFactory.Block_;
using Silk.NET.Maths;

namespace BlockFactory.World_.Interfaces;

public static class BlockStorage
{
    public static void SetBlock(this IBlockStorage access, Vector3D<int> pos, Block block, bool shouldUpdate = true)
    {
        access.SetBlock(pos, (short)block.Id, shouldUpdate);
    }

    public static Block GetBlockObj(this IBlockAccess access, Vector3D<int> pos)
    {
        return Blocks.Registry[access.GetBlock(pos)]!;
    }
}