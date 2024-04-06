using BlockFactory.Entity_;
using BlockFactory.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Menu_;

public class InventoryMenu : SynchronizedMenu
{
    public InventoryMenu(PlayerEntity user) : base(user)
    {
        Root = new SlottedWindowControl(new Vector2D<int>(9, 5),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 8, 0, new LabelControl("Inventory"));
    }
}