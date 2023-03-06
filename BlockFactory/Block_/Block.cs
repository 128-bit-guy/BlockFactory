using BlockFactory.Block_.Instance;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Inventory_;
using BlockFactory.Registry_;
using BlockFactory.Util;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Block_;

public class Block : RegistryEntry
{
    public virtual void AddCollisionBoxes(IBlockReader world, Vector3i pos, BlockState state,
        PhysicsEntity.BoxConsumer consumer, PhysicsEntity entity)
    {
        AddRayCastBoxes(world, pos, state, consumer);
    }

    public virtual BlockState GetPlacementState(Vector3i pos, SlotPointer container, PlayerEntity entity,
        (Vector3i pos, float time, Direction dir) rayCastRes)
    {
        return new BlockState(this, RandomRotations.Any(entity.GameInstance!.Random));
    }

    public virtual void AddRayCastBoxes(IBlockReader world, Vector3i pos, BlockState state,
        PhysicsEntity.BoxConsumer consumer)
    {
        consumer(new Box3(0, 0, 0, 1, 1, 1));
    }

    public virtual BlockInstance? CreateInstance()
    {
        return null;
    }
}