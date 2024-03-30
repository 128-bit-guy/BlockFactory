using BlockFactory.Client;
using BlockFactory.Gui.Control;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Menu_;

public class Menu
{
    public MenuControl? Root;

    public void UpdateAndRender(Box2D<float> workingArea)
    {
        Root?.SetWorkingArea(workingArea);
        Root?.UpdateAndRender(-99);
    }

    public void MouseDown(MouseButton button)
    {
        Root?.MouseDown(button);
    }

    public void MouseUp(MouseButton button)
    {
        Root?.MouseUp(button);
    }

    public void KeyDown(Key key, int a)
    {
        Root?.KeyDown(key, a);
    }

    public void KeyChar(char c)
    {
        Root?.KeyChar(c);
    }

    public virtual void EscapePressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}