using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class Menu
{
    public IMenuControl? Root;
    
    public void UpdateAndRender(Box2D<float> workingArea)
    {
        Root?.SetWorkingArea(workingArea);
        Root?.UpdateAndRender(-99);
    }
}