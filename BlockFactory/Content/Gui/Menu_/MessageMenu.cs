using BlockFactory.Content.Entity_;
using BlockFactory.Content.Gui.Control;
using BlockFactory.Server;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

public class MessageMenu : SynchronizedMenu
{
    private ButtonControl _send;
    private TextInputControl _textInput;
    public MessageMenu(PlayerEntity user) : base(user)
    {
        Root = new SlottedWindowControl(new Vector2D<int>(5, 3),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 4, 0, new LabelControl("Send message"))
            .With(0, 1, 4, 1, _textInput = new TextInputControl())
            .With(0, 2, 4, 2, _send = new ButtonControl("Send"));
        _send.Pressed += SendPressed;
        _textInput.Text = "/";
        _textInput.EnterPressed += EnterPressed;
    }

    private void SendPressed()
    {
        Send();
    }

    private void EnterPressed()
    {
        Send();
    }

    private void Send()
    {
        MenuManager.Pop();
        BlockFactoryServer.ProcessCommand(User, _textInput.Text);
    }
}