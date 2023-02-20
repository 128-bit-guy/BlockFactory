using BlockFactory.Game;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class CredentialsScreen : Screen
{
    public TextInputWidget Name;
    public TextInputWidget Password;
    public ButtonWidget Save;
    private Credentials _credentials;
    public CredentialsScreen(BlockFactoryClient client) : base(client)
    {
        _credentials = client.ClientSettings.Credentials;
    }

    public override void InitWidgets(Vector2i size)
    {
        var (width, height) = size;
        var centerX = width / 2f;
        var centerY = height / 2f;
        Widgets.Add(Name = new TextInputWidget(this,
            new Box2(centerX - 300, centerY - 130, centerX + 300, centerY - 60)));
        Name.Text = _credentials.Name;
        Widgets.Add(Password = new TextInputWidget(this,
            new Box2(centerX - 300, centerY + 30, centerX + 300, centerY + 100)));
        Password.Text = _credentials.Password;
        Widgets.Add(Save = new ButtonWidget(this,
            new Box2(centerX - 300, centerY + 190, centerX + 300, centerY + 260), "Save & go back"));
        Save.OnClick += (_, _) => SaveSettings();
    }

    private void SaveSettings()
    {
        _credentials.Name = Name.Text;
        _credentials.Password = Password.Text;
        Client.SaveSettings();
        Client.PopScreen();
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        var (width, height) = Client.GetDimensions();
        Client.Matrices.Push();
        Client.Matrices.Translate((width / 2f, height / 2 - 360, 1));
        DrawText("Credentials", 0);
        Client.Matrices.Pop();
        Client.Matrices.Push();
        Client.Matrices.Translate((width / 2f, height / 2 - 200, 1));
        DrawText("Name", 0);
        Client.Matrices.Pop();
        Client.Matrices.Push();
        Client.Matrices.Translate((width / 2f, height / 2 - 40, 1));
        DrawText("Password", 0);
        Client.Matrices.Pop();
    }
}