using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public abstract class MenuControl
{
    public MenuControl? Parent;
    public bool IsMouseOver { get; private set; }

    private void UpdateMouse()
    {
        if (Parent is { IsMouseOver: false })
        {
            IsMouseOver = false;
            return;
        }

        IsMouseOver = GetControlBox().Contains(BlockFactoryClient.InputContext.Mice[0].Position.ToGeneric());
    }

    public abstract void SetWorkingArea(Box2D<float> box);

    public virtual void UpdateAndRender(float z)
    {
        UpdateMouse();
    }
    public abstract Box2D<float> GetControlBox();

    public virtual void MouseDown(MouseButton button)
    {
        
    }
} 