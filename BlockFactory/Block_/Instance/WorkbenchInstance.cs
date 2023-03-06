using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Gui;
using BlockFactory.Init;
using OpenTK.Mathematics;

namespace BlockFactory.Block_.Instance;

public class WorkbenchInstance : BlockInstance
{
    public override bool OnUsed(PlayerEntity entity, (Vector3i pos, float time, Direction dir) rayCastRes)
    {
        entity.SwitchMenu(new PlayerInventoryMenu(InGameMenuTypes.PlayerInventory, entity));
        return true;
    }

    public override void Tick()
    {
        Console.WriteLine($"Tick at {Pos}");
    }
}