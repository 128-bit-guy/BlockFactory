using BlockFactory.Gui.Widget;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui.InGame.Widget_;

[ExclusiveTo(Side.Client)]
public class InGameMenuClientWidget<T> : Widget
    where T : InGameMenuWidget
{
    protected readonly T MenuWidget;

    public InGameMenuClientWidget(T menuWidget, InGameMenuScreen screen) : base(screen, GetBox(menuWidget, screen))
    {
        MenuWidget = menuWidget;
    }

    private static Box2 GetBox(T menuWidget, InGameMenuScreen screen)
    {
        var minBox = screen.GetGridCellRectangle(menuWidget.InclusiveBox.Min);
        var maxBox = screen.GetGridCellRectangle(menuWidget.InclusiveBox.Max);
        return new Box2(minBox.Min, maxBox.Max);
    }
}