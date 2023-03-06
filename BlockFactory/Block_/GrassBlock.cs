using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Inventory_;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Block_;

public class GrassBlock : DirtBlock
{
    public override BlockState GetPlacementState(Vector3i pos, SlotPointer container, PlayerEntity entity,
        (Vector3i pos, float time, Direction dir) rayCastRes)
    {
        return new BlockState(this, RandomRotations.KeepingUp(entity.GameInstance!.Random));
    }
}