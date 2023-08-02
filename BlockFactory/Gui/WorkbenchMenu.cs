using BlockFactory.Block_.Instance;
using BlockFactory.Entity_.Player;
using BlockFactory.Gui.Widget;
using BlockFactory.Init;
using OpenTK.Mathematics;
using SharpNoise.Modules;

namespace BlockFactory.Gui;

public class WorkbenchMenu : InGameMenu
{
    private PlayerEntity _entity;
    private WorkbenchInstance _workbench;

    public WorkbenchMenu(InGameMenuType type, BinaryReader reader) : base(type, reader)
    {
        _entity = null!;
        _workbench = null!;
    }

    public WorkbenchMenu(InGameMenuType type, PlayerEntity player, WorkbenchInstance workbench) : base(type)
    {
        _entity = player;
        _workbench = workbench;
        Size = new Vector2i(9, 10);
        AddWidget(new LabelWidget(InGameMenuWidgetTypes.Label, new Box2i(0, 0, 8, 0),
            "Workbench", -1));
        AddWidget(new LabelWidget(InGameMenuWidgetTypes.Label, new Box2i(0, 4, 8, 4),
            "Inventory", -1));
        AddWidget(new LabelWidget(InGameMenuWidgetTypes.Label, new Box2i(0, 8, 8, 8),
            "Hotbar", -1));
        var inventory = new SlotGroup();
        var hotbar = new SlotGroup();
        var crafting = new SlotGroup();
        for (var i = 0; i < 9; ++i)
            AddWidget(new SlotWidget(InGameMenuWidgetTypes.Slot, (i, 9), player.Hotbar, i, hotbar));
        for (var j = 0; j < 3; ++j)
        for (var i = 0; i < 9; ++i)
            AddWidget(new SlotWidget(InGameMenuWidgetTypes.Slot, (i, 5 + j),
                player.Inventory, j * 9 + i, inventory));
        for (var j = 0; j < 3; ++j)
        for (var i = 0; i < 3; ++i)
            AddWidget(new SlotWidget(InGameMenuWidgetTypes.Slot, (i + 1, j + 1), workbench.Inventory, j * 3 + i,
                crafting));
        inventory.Next = hotbar;
        hotbar.Next = inventory;
        crafting.Next = inventory;
    }
}