using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Init;
using BlockFactory.Inventory_;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Item_;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block)
    {
        Block = block;
    }

    public override bool OnUse(IStackContainer container, PlayerEntity entity,
        (Vector3i pos, float time, Direction dir)? rayCastRes)
    {
        if (Block != Blocks.Air && rayCastRes.HasValue)
        {
            var (blockPos, time, dir) = rayCastRes.Value;
            entity.World!.SetBlockState(blockPos + dir.GetOffset(), new BlockState(Block,
                RandomRotations.Any(entity.GameInstance!.Random)));
            entity.AddUseCooldown(3);
            container.ChangeStack(container.GetStack() - 1);
            return true;
        }

        return false;
    }
}