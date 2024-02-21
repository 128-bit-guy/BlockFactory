using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class MenuManager
{
    private readonly Stack<Menu> _menus = new();
    private MenuSwitchAnimationType _animationType = MenuSwitchAnimationType.None;
    private Menu? _previousMenu = null;
    private float _progress = 0;

    public bool Empty => _menus.Count == 0;

    public void Push(Menu menu)
    {
        var previous = _menus.Count == 0 ? null : _menus.Peek();
        _menus.Push(menu);
        _animationType = MenuSwitchAnimationType.Push;
        _progress = 0;
        _previousMenu ??= previous;
    }

    public void Pop()
    {
        var previous = _menus.Pop();
        _animationType = MenuSwitchAnimationType.Pop;
        _progress = 0;
        _previousMenu ??= previous;
    }

    public Menu? Top => _menus.Count == 0 ? null : _menus.Peek();

    public void UpdateAndRender(double deltaTime)
    {
        var size = BlockFactoryClient.Window.FramebufferSize;
        Menu? newMenu;
        Menu? oldMenu;
        float progress;
        switch (_animationType)
        {
            case MenuSwitchAnimationType.None:
                newMenu = Top;
                oldMenu = null;
                progress = 1;
                break;
            case MenuSwitchAnimationType.Push:
                newMenu = Top;
                oldMenu = _previousMenu;
                progress = _progress;
                break;
            case MenuSwitchAnimationType.Pop:
                newMenu = _previousMenu!;
                oldMenu = Top;
                progress = 1 - _progress;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (oldMenu != null)
        {
            var delta = -size.X * progress;
            oldMenu.UpdateAndRender(new Box2D<float>(delta, 0, size.X + delta, size.Y));
        }

        if (newMenu != null)
        {
            var delta = -size.X * (progress - 1);
            newMenu.UpdateAndRender(new Box2D<float>(delta, 0, size.X + delta, size.Y));
        }

        if (_animationType != MenuSwitchAnimationType.None)
        {
            _progress += (float)deltaTime * 3;
            if(_progress < 1) return;
            _progress = 0;
            _animationType = MenuSwitchAnimationType.None;
            _previousMenu = null;
        }
    }
    
    public bool IsAnimationPlaying()
    {
        return _animationType != MenuSwitchAnimationType.None;
    }

    public bool HasAnythingToRender()
    {
        return !Empty || IsAnimationPlaying();
    }
}