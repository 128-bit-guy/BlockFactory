using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

[ExclusiveTo(Side.Client)]
public class MultiplayerMenu : Menu
{
    private readonly ButtonControl _back;
    private readonly ButtonControl _connect;
    private readonly TextInputControl _textInput;

    public MultiplayerMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(4, 4),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 3, 0, new LabelControl("Multiplayer"))
            .With(0, 1, 3, 1, _textInput = new TextInputControl())
            .With(0, 2, 3, 2, _connect = new ButtonControl("Connect"))
            .With(0, 3, 3, 3, _back = new ButtonControl("Back"));
        _connect.Pressed += OnConnectPressed;
        _back.Pressed += OnBackPressed;
        _textInput.EnterPressed += OnEnterPressed;
    }

    private void OnConnectPressed()
    {
        Connect();
    }

    private void OnEnterPressed()
    {
        Connect();
    }

    private void Connect()
    {
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.StartMultiplayer(_textInput.Text);
    }

    private void OnBackPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}