using BlockFactory.Client.Gui.Singleplayer;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class MainMenuScreen : Screen
{
    public ButtonWidget CreditsButton = null!;
    public ButtonWidget ModsButton = null!;
    public ButtonWidget MultiplayerButton = null!;
    public ButtonWidget QuitButton = null!;
    public ButtonWidget ResourcePacksButton = null!;
    public ButtonWidget SettingsButton = null!;
    public ButtonWidget SingleplayerButton = null!;

    public MainMenuScreen(BlockFactoryClient client) : base(client)
    {
    }

    public override void InitWidgets(Vector2i size)
    {
        base.InitWidgets(size);
        var (width, height) = size;
        var centerX = width / 2f;
        var centerY = height / 2f;
        Widgets.Add(SingleplayerButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY - 210, centerX + 300, centerY - 140), "Singleplayer"));
        SingleplayerButton.OnClick += (_, _) => Client.PushScreen(new WorldSelectionScreen(Client));
        Widgets.Add(MultiplayerButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY - 130, centerX + 300, centerY - 60), "Multiplayer"));
        MultiplayerButton.OnClick += (_, _) => Client.PushScreen(new ServerScreen(Client));
        Widgets.Add(SettingsButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY - 50, centerX + 300, centerY + 20), "Settings"));
        SettingsButton.OnClick += (_, _) => Client.PushScreen(new CredentialsScreen(Client));
        Widgets.Add(ResourcePacksButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY + 30, centerX + 300, centerY + 100), "Resource Packs"));
        ResourcePacksButton.Enabled = false;
        Widgets.Add(ModsButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY + 110, centerX - 5, centerY + 180), "Mods"));
        ModsButton.Enabled = false;
        Widgets.Add(CreditsButton = new ButtonWidget(this,
            new Box2(centerX + 5, centerY + 110, centerX + 300, centerY + 180), "Credits"));
        CreditsButton.OnClick += (_, _) => Client.PushScreen(new CreditsScreen(Client));
        Widgets.Add(QuitButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY + 190, centerX + 300, centerY + 260), "Quit"));
        QuitButton.OnClick += (_, _) => Client.Stop();
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        var (width, height) = Client.GetDimensions();
        Client.Matrices.Push();
        Client.Matrices.Translate((width / 2f, height / 2 - 360, 1));
        Client.Matrices.Scale(2);
        DrawText("Block Factory", 0);
        Client.Matrices.Pop();
    }
}