using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class SettingsMenu : Menu
{
    private readonly ButtonControl _credentials;
    private readonly ButtonControl _back;
    public SettingsMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(4, 3),
                Array.Empty<int>(), new int[] { 1 })
            .With(0, 0, 3, 0, new LabelControl("Settings"))
            .With(0, 1, 3, 1, _credentials = new ButtonControl("Credentials"))
            .With(0, 2, 3, 2, _back = new ButtonControl("Back"));
        _credentials.Pressed += OnCredentialsPressed;
        _back.Pressed += OnBackPressed;
    }

    private void OnCredentialsPressed()
    {
        BlockFactoryClient.MenuManager.Push(new CredentialsMenu());
    }

    private void OnBackPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.SaveSettings();
    }

    public override void EscapePressed()
    {
        base.EscapePressed();
        BlockFactoryClient.SaveSettings();
    }
}