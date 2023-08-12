using BlockFactory.Entity_.Player;
using BlockFactory.Init;
using BlockFactory.Item_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Block_;

public class StoneBlock : Block
{
    public override void AddDroppedStacks(IBlockReader world, Vector3i pos, BlockState state, PlayerEntity breaker, List<ItemStack> stacks)
    {
        stacks.Add(new ItemStack(Items.BlockItems[Blocks.Cobblestone]));
    }
}