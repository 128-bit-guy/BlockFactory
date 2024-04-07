using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_;

namespace BlockFactory.Item_;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block)
    {
        Block = block;
    }

    public override void Use(ItemStack stack, BlockPointer pointer, CubeFace face, object user)
    {
        var placePos = pointer + face.GetDelta();
        if (placePos.GetBlock() == Blocks.Air.Id || placePos.GetBlock() == Blocks.Water.Id)
        {
            placePos.SetBlock(Block);
            stack.Decrement();
        }
    }
}