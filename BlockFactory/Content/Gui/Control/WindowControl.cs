using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Control;

public abstract class WindowControl : SynchronizedMenuControl
{
    [ExclusiveTo(Side.Client)]
    private const float Padding = 16f;
    
    [ExclusiveTo(Side.Client)]
    private Box2D<float> _controlBox;

    [ExclusiveTo(Side.Client)]
    protected abstract Vector2D<float> GetSize();

    [ExclusiveTo(Side.Client)]
    public override void SetWorkingArea(Box2D<float> box)
    {
        var size = GetSize();
        var min = box.Center - size / 2;
        var contentBox = new Box2D<float>(min, min + size);
        _controlBox = new Box2D<float>(contentBox.Min - new Vector2D<float>(Padding),
            contentBox.Max + new Vector2D<float>(Padding));
        SetChildrenAreas(contentBox);
    }

    [ExclusiveTo(Side.Client)]
    protected abstract void SetChildrenAreas(Box2D<float> contentBox);

    [ExclusiveTo(Side.Client)]
    public override void UpdateAndRender(float z)
    {
        base.UpdateAndRender(z);
        RenderWindow(z);
        UpdateAndRenderChildren(z + 1);
    }

    [ExclusiveTo(Side.Client)]
    private void RenderWindow(float z)
    {
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(0, 0, z);
        var box = GetControlBox();
        GuiRenderHelper.RenderQuadWithBorder(3, box, Padding, 0.25f);
        BfRendering.Matrices.Pop();
    }

    [ExclusiveTo(Side.Client)]
    protected abstract void UpdateAndRenderChildren(float z);

    [ExclusiveTo(Side.Client)]
    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }
}