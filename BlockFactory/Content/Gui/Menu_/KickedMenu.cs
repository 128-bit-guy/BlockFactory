using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

[ExclusiveTo(Side.Client)]
public class KickedMenu : Menu
{
    private readonly ButtonControl _back;

    public KickedMenu(string s)
    {
        Root = new SlottedWindowControl(new Vector2D<int>(12, 3),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 11, 0, new LabelControl("You were disconnected from server"))
            .With(0, 1, 11, 1, new LabelControl(s))
            .With(0, 2, 11, 2, _back = new ButtonControl("Back"));
        _back.Pressed += OnBackPressed;
    }

    private void OnBackPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}