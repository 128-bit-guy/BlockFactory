using BlockFactory.Entity_.Player;
using BlockFactory.Gui.Widget;
using BlockFactory.Init;
using OpenTK.Mathematics;

namespace BlockFactory.Gui;

public class PlayerInventoryMenu : InGameMenu
{
    public PlayerInventoryMenu(InGameMenuType type, BinaryReader reader) : base(type, reader)
    {
    }

    public PlayerInventoryMenu(InGameMenuType type, PlayerEntity player) : base(type)
    {
        Size = new Vector2i(9, 6);
        AddWidget(new LabelWidget(InGameMenuWidgetTypes.Label, new Box2i(0, 0, 8, 0),
            "Inventory", -1));
        AddWidget(new LabelWidget(InGameMenuWidgetTypes.Label, new Box2i(0, 4, 8, 4),
            "Hotbar", -1));
        var inventory = new SlotGroup();
        var hotbar = new SlotGroup();
        for (var i = 0; i < 9; ++i)
            AddWidget(new SlotWidget(InGameMenuWidgetTypes.Slot, (i, 5), player.Hotbar, i, hotbar));
        for (var j = 0; j < 3; ++j)
        for (var i = 0; i < 9; ++i)
            AddWidget(new SlotWidget(InGameMenuWidgetTypes.Slot, (i, 1 + j),
                player.Inventory, j * 9 + i, inventory));
        inventory.Next = hotbar;
        hotbar.Next = inventory;
    }
}