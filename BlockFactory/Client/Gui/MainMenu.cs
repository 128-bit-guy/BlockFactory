using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class MainMenu : Menu
{
    public MainMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(6, 5),
            Array.Empty<int>(), new[]{3})
            .With(new Box2D<int>(0, 0, 5, 0), new LabelControl("Block Factory"))
            .With(new Box2D<int>(0, 1, 5, 1), new ButtonControl("Singleplayer"))
            .With(new Box2D<int>(0, 2, 5, 2), new ButtonControl("Multiplayer"))
            .With(new Box2D<int>(0, 3, 5, 3), new ButtonControl("Options"))
            .With(new Box2D<int>(0, 4, 1, 4), new ButtonControl("Mods"))
            .With(new Box2D<int>(2, 4, 5, 4), new ButtonControl("Textures"));
    }
}