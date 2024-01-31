using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Client.Render.Texture_;
using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public abstract class WindowControl : IMenuControl
{
    private const float Padding = 16f;
    private Box2D<float> _controlBox;

    protected abstract Vector2D<float> GetSize();

    public void SetWorkingArea(Box2D<float> box)
    {
        var size = GetSize();
        var min = box.Center - (size / 2);
        var contentBox = new Box2D<float>(min, min + size);
        _controlBox = new Box2D<float>(contentBox.Min - new Vector2D<float>(Padding),
            contentBox.Max + new Vector2D<float>(Padding));
        SetChildrenAreas(contentBox);
    }

    protected abstract void SetChildrenAreas(Box2D<float> contentBox);

    public void UpdateAndRender(float z)
    {
        RenderWindow(z);
        UpdateAndRenderChildren(z + 1);
    }

    private void RenderWindow(float z)
    {
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(0, 0, z);
        var box = GetControlBox();
        GuiRenderHelper.RenderQuadWithBorder(Textures.Window, box, Padding, 0.25f);
        BfRendering.Matrices.Pop();
    }

    protected abstract void UpdateAndRenderChildren(float z);
    public Box2D<float> GetControlBox()
    {
        return _controlBox;
    }
}