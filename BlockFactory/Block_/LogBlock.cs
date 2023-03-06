using BlockFactory.Block_.Instance;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Inventory_;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Block_;

public class LogBlock : Block
{
    public override BlockState GetPlacementState(Vector3i pos, SlotPointer container, PlayerEntity entity,
        (Vector3i pos, float time, Direction dir) rayCastRes)
    {
        return new BlockState(this,
            CubeRotation.GetFromTo(Direction.Up, rayCastRes.dir)[0] *
            RandomRotations.KeepingY(entity.GameInstance!.Random));
    }
}