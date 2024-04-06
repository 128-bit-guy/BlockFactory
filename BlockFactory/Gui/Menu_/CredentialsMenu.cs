using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Menu_;

[ExclusiveTo(Side.Client)]
public class CredentialsMenu : Menu
{
    private readonly ButtonControl _back;
    private readonly TextInputControl _name;
    private readonly TextInputControl _password;

    public CredentialsMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(8, 4),
                Array.Empty<int>(), new[] { 2 })
            .With(0, 0, 7, 0, new LabelControl("Credentials"))
            .With(0, 1, 3, 1, new LabelControl("Name:"))
            .With(4, 1, 7, 1, _name = new TextInputControl())
            .With(0, 2, 3, 2, new LabelControl("Password:"))
            .With(4, 2, 7, 2, _password = new TextInputControl())
            .With(0, 3, 7, 3, _back = new ButtonControl("Back"));
        _name.Text = BlockFactoryClient.Settings.Credentials.Name;
        _name.TextChanged += OnNameTextChanged;
        _password.TextChanged += OnPasswordTextChanged;
        _password.Text = BlockFactoryClient.Settings.Credentials.Password;
        _back.Pressed += OnBackPressed;
    }

    private void OnNameTextChanged()
    {
        BlockFactoryClient.Settings.Credentials.Name = _name.Text;
        UpdateBack();
    }

    private void OnPasswordTextChanged()
    {
        BlockFactoryClient.Settings.Credentials.Password = _password.Text;
    }

    private void UpdateBack()
    {
        _back.Enabled = _name.Text.Length > 0 && _password.Text.Length > 0;
    }

    private void OnBackPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }

    public override void EscapePressed()
    {
        if (_back.Enabled) base.EscapePressed();
    }
}