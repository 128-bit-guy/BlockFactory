using BlockFactory.Content.Entity_;
using BlockFactory.Content.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

public class InventoryMenu : SynchronizedMenu
{
    public InventoryMenu(PlayerEntity user) : base(user)
    {
        var win = new SlottedWindowControl(new Vector2D<int>(9, 5),
                Array.Empty<int>(), new []{3})
            .With(0, 0, 8, 0, new LabelControl("Inventory"));
        Root = win;
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 9; ++j)
        {
            win.With(j, i + 1, new ItemSlotControl(user.Inventory, i * 9 + j));
        }

        for (var i = 0; i < 9; ++i)
        {
            win.With(i, 4, new ItemSlotControl(user.HotBar, i));
        }
    }
}