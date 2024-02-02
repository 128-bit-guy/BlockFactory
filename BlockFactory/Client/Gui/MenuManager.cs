using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class MenuManager
{
    private readonly Stack<Menu> _menus = new();

    public bool Empty => _menus.Count == 0;

    public void Push(Menu menu)
    {
        _menus.Push(menu);
    }

    public void Pop()
    {
        _menus.Pop();
    }

    public Menu? Top => _menus.Count == 0 ? null : _menus.Peek();

    public void UpdateAndRender()
    {
        if (Empty) return;
        var size = BlockFactoryClient.Window.FramebufferSize;
        Top!.UpdateAndRender(new Box2D<float>(0, 0, size.X, size.Y));
    }
}