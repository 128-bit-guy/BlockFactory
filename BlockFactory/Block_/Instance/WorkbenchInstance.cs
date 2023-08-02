using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Gui;
using BlockFactory.Init;
using BlockFactory.Inventory_;
using OpenTK.Mathematics;

namespace BlockFactory.Block_.Instance;

public class WorkbenchInstance : BlockInstance
{
    public SimpleInventory Inventory;

    public WorkbenchInstance()
    {
        Inventory = new SimpleInventory(9);
    }
    public override bool OnUsed(PlayerEntity entity, (Vector3i pos, float time, Direction dir) rayCastRes)
    {
        entity.SwitchMenu(new WorkbenchMenu(InGameMenuTypes.PlayerInventory, entity, this));
        return true;
    }

    public override void Tick()
    {
        // Console.WriteLine($"Tick at {Pos}");
    }
}