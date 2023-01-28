using System.Diagnostics;
using BlockFactory.Client.Gui.InGame.Widget_;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Gui;

namespace BlockFactory.Client.Gui.InGame;

public class InGameMenuScreen : Screen
{
    public readonly InGameMenu Menu;
    public const int GridCellSize = 64;
    public const int GridCellPadding = 8;
    public const int WindowSizeExtension = 20;
    public InGameMenuScreen(InGameMenu menu, BlockFactoryClient client) : base(client)
    {
        Menu = menu;
    }

    private Vector2 GetMinGridCellPos()
    {
        (int width, int height) = Client.GetDimensions();
        Vector2 center = new Vector2(width, height) * (1 / 2f);
        Vector2 gridSize = Menu.Size.ToVector2() * (GridCellSize + GridCellPadding) - new Vector2(GridCellPadding);
        return center - (gridSize * (1 / 2f));
    }

    public Box2 GetGridCellRectangle(Vector2i pos)
    {
        Vector2 minGridCellPos = GetMinGridCellPos();
        Vector2 cellMinPos = minGridCellPos + pos.ToVector2() * (GridCellSize + GridCellPadding);
        Vector2 cellMaxPos = cellMinPos + new Vector2(GridCellSize);
        return new Box2(cellMinPos, cellMaxPos);
    }

    public Box2 GetWindowRectangle()
    {
        Vector2 min = GetGridCellRectangle(Vector2i.Zero).Min - new Vector2(WindowSizeExtension);
        Vector2 max = GetGridCellRectangle(Menu.Size - Vector2i.One).Max + new Vector2(WindowSizeExtension);
        return new Box2(min, max);
    }
    
    public override void InitWidgets(Vector2i size)
    {
        base.InitWidgets(size);
        foreach (var menuWidget in Menu.Widgets)
        {
            Widgets.Add(InGameMenuClientWidgets.ClientWidgetCreators[menuWidget.Type](menuWidget, this));
        }
    }

    public override unsafe void DrawWindow()
    { 
        base.DrawWindow();
        {
            Box2 box = GetWindowRectangle();
            DrawColoredRect(box, 0, (0.75f, 0.75f, 0.75f, 1f));
            DrawColoredRect(new(box.Min, (box.Max.X, box.Min.Y + 5)), 0.5f, (0, 0, 0, 1));
            DrawColoredRect(new(box.Min, (box.Min.X + 5, box.Max.Y)), 0.5f, (0, 0, 0, 1));
            DrawColoredRect(new((box.Min.X, box.Max.Y - 5), box.Max), 0.5f, (0, 0, 0, 1));
            DrawColoredRect(new((box.Max.X - 5, box.Min.Y), box.Max), 0.5f, (0, 0, 0, 1));
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