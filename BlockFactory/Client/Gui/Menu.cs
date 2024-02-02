using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

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

    public virtual void EscapePressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}