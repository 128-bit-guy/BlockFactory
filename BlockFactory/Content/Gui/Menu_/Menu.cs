using BlockFactory.Base;
using BlockFactory.Content.Gui.Control;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

public class Menu
{
    public MenuManager MenuManager = null!;
    private MenuControl? _root;

    public MenuControl? Root
    {
        get => _root;
        set
        {
            _root = value;
            value!.ParentMenu = this;
        }
    }

    [ExclusiveTo(Side.Client)]
    public virtual void UpdateAndRender(Box2D<float> workingArea)
    {
        Root?.SetWorkingArea(workingArea);
        Root?.UpdateAndRender(-99);
    }

    [ExclusiveTo(Side.Client)]
    public void MouseDown(MouseButton button)
    {
        Root?.MouseDown(button);
    }

    [ExclusiveTo(Side.Client)]
    public void MouseUp(MouseButton button)
    {
        Root?.MouseUp(button);
    }

    [ExclusiveTo(Side.Client)]
    public void KeyDown(Key key, int a)
    {
        Root?.KeyDown(key, a);
    }

    [ExclusiveTo(Side.Client)]
    public void KeyChar(char c)
    {
        Root?.KeyChar(c);
    }

    public virtual void EscapePressed()
    {
        MenuManager.Pop();
    }
}