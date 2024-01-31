using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public interface IMenuControl
{
    public void SetWorkingArea(Box2D<float> box);
    public void UpdateAndRender(float z);
    public Box2D<float> GetControlBox();
}