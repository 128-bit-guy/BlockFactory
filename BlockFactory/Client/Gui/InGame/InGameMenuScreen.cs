using BlockFactory.Client.Gui.InGame.Widget_;
using BlockFactory.Gui;
using BlockFactory.Side_;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Gui.InGame;

[ExclusiveTo(Side.Client)]
public class InGameMenuScreen : Screen
{
    public const int GridCellSize = 64;
    public const int GridCellPadding = 8;
    public const int WindowSizeExtension = 20;
    public readonly InGameMenu Menu;

    public InGameMenuScreen(InGameMenu menu, BlockFactoryClient client) : base(client)
    {
        Menu = menu;
    }

    private Vector2 GetMinGridCellPos()
    {
        var (width, height) = Client.GetDimensions();
        var center = new Vector2(width, height) * (1 / 2f);
        var gridSize = Menu.Size.ToVector2() * (GridCellSize + GridCellPadding) - new Vector2(GridCellPadding);
        return center - gridSize * (1 / 2f);
    }

    public Box2 GetGridCellRectangle(Vector2i pos)
    {
        var minGridCellPos = GetMinGridCellPos();
        var cellMinPos = minGridCellPos + pos.ToVector2() * (GridCellSize + GridCellPadding);
        var cellMaxPos = cellMinPos + new Vector2(GridCellSize);
        return new Box2(cellMinPos, cellMaxPos);
    }

    public Box2 GetWindowRectangle()
    {
        var min = GetGridCellRectangle(Vector2i.Zero).Min - new Vector2(WindowSizeExtension);
        var max = GetGridCellRectangle(Menu.Size - Vector2i.One).Max + new Vector2(WindowSizeExtension);
        return new Box2(min, max);
    }

    public override void InitWidgets(Vector2i size)
    {
        base.InitWidgets(size);
        foreach (var menuWidget in Menu.Widgets)
            Widgets.Add(InGameMenuClientWidgets.ClientWidgetCreators[menuWidget.Type](menuWidget, this));
    }

    public override unsafe void DrawWindow()
    {
        base.DrawWindow();
        {
            var box = GetWindowRectangle();
            DrawColoredRect(box, 0, (0.75f, 0.75f, 0.75f, 1f));
            DrawColoredRect(new Box2(box.Min, (box.Max.X, box.Min.Y + 5)), 0.5f, (0, 0, 0, 1));
            DrawColoredRect(new Box2(box.Min, (box.Min.X + 5, box.Max.Y)), 0.5f, (0, 0, 0, 1));
            DrawColoredRect(new Box2((box.Min.X, box.Max.Y - 5), box.Max), 0.5f, (0, 0, 0, 1));
            DrawColoredRect(new Box2((box.Max.X - 5, box.Min.Y), box.Max), 0.5f, (0, 0, 0, 1));
        }
        GLFW.GetCursorPos(Client.Window, out var x, out var y);
        Client.Matrices.Push();
        Client.Matrices.Translate(new Vector3((float)x, (float)y, 10f));
        DrawStack(Menu.MovedStackInventory[0]);
        Client.Matrices.Pop();
        // for (int i = 0; i < Menu.Size.X; ++i)
        // {
        //     for (int j = 0; j < Menu.Size.Y; ++j)
        //     {
        //         Vector2i pos = (i, j);
        //         Box2 Box = GetGridCellRectangle(pos);
        //     }
        // }
    }
}