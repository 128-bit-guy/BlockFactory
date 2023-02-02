using BlockFactory.Gui.Widget;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui.InGame.Widget_;

[ExclusiveTo(Side.Client)]
public class LabelClientWidget : InGameMenuClientWidget<LabelWidget>
{
    public LabelClientWidget(LabelWidget menuWidget, InGameMenuScreen screen) : base(menuWidget, screen)
    {
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        Vector2 pos = default;
        pos.Y = Box.Min.Y;
        switch (MenuWidget.Centering)
        {
            case -1:
                pos.X = Box.Min.X;
                break;
            case 0:
                pos.X = (Box.Min.X + Box.Max.X) / 2;
                break;
            case 1:
                pos.X = Box.Max.X;
                break;
        }

        Screen.Client.Matrices.Push();
        Screen.Client.Matrices.Translate(new Vector3(pos.X, pos.Y, 1f));
        Screen.DrawText(MenuWidget.Text, MenuWidget.Centering);
        Screen.Client.Matrices.Pop();
    }
}