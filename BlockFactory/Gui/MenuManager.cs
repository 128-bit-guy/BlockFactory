using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Gui.Menu_;
using Silk.NET.Maths;

namespace BlockFactory.Gui;

public class MenuManager
{
    private readonly Stack<Menu> _menus = new();
    [ExclusiveTo(Side.Client)]
    private MenuSwitchAnimationType _animationType;
    [ExclusiveTo(Side.Client)]
    private Menu? _previousMenu;
    [ExclusiveTo(Side.Client)]
    private float _progress;

    public readonly bool Server;

    public MenuManager(bool server)
    {
        Server = server;
    }

    public bool Empty => _menus.Count == 0;

    public Menu? Top => _menus.Count == 0 ? null : _menus.Peek();

    public virtual void Push(Menu menu)
    {
        var previous = _menus.Count == 0 ? null : _menus.Peek();
        _menus.Push(menu);
        menu.MenuManager = this;
        if (!Server)
        {
            _animationType = MenuSwitchAnimationType.Push;
            _progress = 0;
            _previousMenu ??= previous;
        }
    }

    public virtual void Pop()
    {
        var previous = _menus.Pop();
        if (!Server)
        {
            _animationType = MenuSwitchAnimationType.Pop;
            _progress = 0;
            _previousMenu ??= previous;
        }
    }

    [ExclusiveTo(Side.Client)]
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
            if (_progress < 1) return;
            _progress = 0;
            _animationType = MenuSwitchAnimationType.None;
            _previousMenu = null;
        }
    }

    [ExclusiveTo(Side.Client)]
    public bool IsAnimationPlaying()
    {
        return _animationType != MenuSwitchAnimationType.None;
    }

    [ExclusiveTo(Side.Client)]
    public bool HasAnythingToRender()
    {
        return !Empty || IsAnimationPlaying();
    }
}