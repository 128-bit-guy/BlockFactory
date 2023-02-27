using BlockFactory.Client.Render;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Gui.Singleplayer;

[ExclusiveTo(Side.Client)]
public class WorldSelectionWidget : Widget
{
    private readonly List<string> _worlds;
    private readonly string _worldsDirectory;
    private int? _selected;
    private bool _unselectNextTick;

    public WorldSelectionWidget(Screen screen, Box2 box) : base(screen, box)
    {
        screen.Client.OnMouseButton += OnMouseButton;
        _worldsDirectory = screen.Client.WorldsDirectory;
        Directory.CreateDirectory(_worldsDirectory);
        _worlds = new List<string>();
        foreach (var directory in Directory.EnumerateDirectories(_worldsDirectory))
            _worlds.Add(Path.GetRelativePath(_worldsDirectory, directory));

        _unselectNextTick = false;
    }

    public override void Dispose()
    {
        base.Dispose();
        Screen.Client.OnMouseButton -= OnMouseButton;
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        if (_unselectNextTick)
        {
            _selected = null;
            _unselectNextTick = false;
        }

        Screen.GuiMesh.Builder.Color = new Vector3(0.6f, 0.6f, 0.6f);
        Screen.DrawTexturedRect(Box, ZIndex, 64, Textures.DirtTexture);
        Screen.DrawColoredRect(new Box2(Box.Min, (Box.Max.X, Box.Min.Y + 5)), ZIndex + 0.5f, (0, 0, 0, 1));
        Screen.DrawColoredRect(new Box2(Box.Min, (Box.Min.X + 5, Box.Max.Y)), ZIndex + 0.5f, (0, 0, 0, 1));
        Screen.DrawColoredRect(new Box2((Box.Min.X, Box.Max.Y - 5), Box.Max), ZIndex + 0.5f, (0, 0, 0, 1));
        Screen.DrawColoredRect(new Box2((Box.Max.X - 5, Box.Min.Y), Box.Max), ZIndex + 0.5f, (0, 0, 0, 1));
        GL.Enable(EnableCap.StencilTest);
        GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Screen.DrawColoredRect(Box, ZIndex, (0, 0, 0, 0));
        GL.Disable(EnableCap.Blend);
        GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
        RenderList();
        GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
        GL.Disable(EnableCap.StencilTest);
    }

    private void RenderList()
    {
        Screen.Client.Matrices.Push();
        Screen.Client.Matrices.Translate((Box.Min.X + 5, Box.Min.Y + 5, ZIndex));
        var i = 0;
        foreach (var s in _worlds)
        {
            Screen.Client.Matrices.Push();
            Screen.Client.Matrices.Translate((0, i * 70, 2));
            Screen.Client.Matrices.Push();
            Screen.Client.Matrices.Translate((5, 0, 1));

            Screen.DrawText(s, -1);
            Screen.Client.Matrices.Pop();
            if (i == _selected)
            {
                Screen.Client.Matrices.Push();
                Screen.DrawTexturedRect(
                    new Box2(0, 0, Box.Size.X, 70),
                    0,
                    64,
                    Textures.StoneTexture
                );
                Screen.Client.Matrices.Pop();
            }

            Screen.Client.Matrices.Pop();
            ++i;
        }

        Screen.Client.Matrices.Pop();
    }

    private unsafe void OnMouseButton(MouseButton button, InputAction action, KeyModifiers modifiers)
    {
        if (!IsMouseOver())
        {
            _unselectNextTick = true;
            return;
        }

        var minPos = Box.Min.X + 5;
        var i = 0;
        GLFW.GetCursorPos(Screen.Client.Window, out var x, out var y);
        foreach (var world in _worlds)
        {
            var cMinPos = minPos + i * 70;
            var cMaxPos = cMinPos + 70;
            if (y >= cMinPos && y <= cMaxPos)
            {
                _selected = i;
                return;
            }

            ++i;
        }

        _unselectNextTick = true;
    }

    public string? GetSelectedPath()
    {
        if (_selected != null && _selected.Value < _worlds.Count)
            return Path.Combine(_worldsDirectory, _worlds[_selected.Value]);

        return null;
    }

    public string? GetSelectedName()
    {
        if (_selected != null && _selected.Value < _worlds.Count) return _worlds[_selected.Value];

        return null;
    }
}