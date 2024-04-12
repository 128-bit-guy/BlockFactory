using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Gui.Menu_;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Control;

public abstract class MenuControl
{
    [ExclusiveTo(Side.Client)]
    public static MenuControl? ActiveControl;
    public MenuControl? Parent;
    public Menu? ParentMenu;
    [ExclusiveTo(Side.Client)]
    public bool IsMouseOver { get; private set; }

    [ExclusiveTo(Side.Client)]
    private void UpdateMouse()
    {
        if (Parent is { IsMouseOver: false })
        {
            IsMouseOver = false;
            return;
        }

        IsMouseOver = GetControlBox().Contains(BlockFactoryClient.InputContext.Mice[0].Position.ToGeneric());
    }

    [ExclusiveTo(Side.Client)]
    public abstract void SetWorkingArea(Box2D<float> box);

    [ExclusiveTo(Side.Client)]
    public virtual void UpdateAndRender(float z)
    {
        UpdateMouse();
    }

    [ExclusiveTo(Side.Client)]
    public abstract Box2D<float> GetControlBox();

    [ExclusiveTo(Side.Client)]
    public virtual void MouseDown(MouseButton button)
    {
    }

    [ExclusiveTo(Side.Client)]
    public virtual void MouseUp(MouseButton button)
    {
    }

    [ExclusiveTo(Side.Client)]
    public virtual void KeyDown(Key key, int a)
    {
    }

    [ExclusiveTo(Side.Client)]
    public virtual void KeyChar(char c)
    {
    }
}