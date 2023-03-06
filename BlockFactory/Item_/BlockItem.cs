using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Init;
using BlockFactory.Inventory_;
using BlockFactory.Util;
using BlockFactory.Util.Math_;
using OpenTK.Mathematics;

namespace BlockFactory.Item_;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block)
    {
        Block = block;
    }

    public override bool OnUse(SlotPointer container, PlayerEntity entity,
        (Vector3i pos, float time, Direction dir)? rayCastRes)
    {
        if (Block == Blocks.Air || !rayCastRes.HasValue) return false;
        var (blockPos, time, dir) = rayCastRes.Value;
        var placePos = blockPos + dir.GetOffset();
        if (entity.Chunk!.Neighbourhood.GetBlockState(placePos).Block != Blocks.Air) return false;
        var placeState = new BlockState(Block, RandomRotations.Any(entity.GameInstance!.Random));
        var center = new EntityPos(placePos);
        var b = new Box3(new Vector3(-2), new Vector3(2));
        if (entity.Chunk!.Neighbourhood.GetInBoxEntityEnumerable(center, b).Where(e => e is PhysicsEntity)
            .Cast<PhysicsEntity>().Any(pe => pe.DoesBlockIntersect(placePos, placeState)))
        {
            return false;
        }

        if (container.TryExtractStack(1, false).Count != 1) return false;
        entity.Chunk!.Neighbourhood.SetBlockState(placePos, placeState);
        entity.AddUseCooldown(3);
        return true;
    }
}