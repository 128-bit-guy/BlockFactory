﻿using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_;

namespace BlockFactory.Content.Item_;

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
        if (placePos.GetBlockObj().IsReplaceable())
        {
            placePos.SetBlock(Block);
            stack.Decrement();
        }
    }
}