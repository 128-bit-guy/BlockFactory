using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class MainMenu : Menu
{
    private readonly ButtonControl _singlePlayer;
    private readonly ButtonControl _multiplayer;
    private readonly ButtonControl _options;
    private readonly ButtonControl _mods;
    private readonly ButtonControl _textures;
    public MainMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(6, 5),
            Array.Empty<int>(), new[]{3})
            .With(0, 0, 5, 0, new LabelControl("Block Factory"))
            .With(0, 1, 5, 1, _singlePlayer = new ButtonControl("Singleplayer"))
            .With(0, 2, 5, 2, _multiplayer = new ButtonControl("Multiplayer"))
            .With(0, 3, 5, 3, _options = new ButtonControl("Options"))
            .With(0, 4, 2, 4, _mods = new ButtonControl("Mods"))
            .With(3, 4, 5, 4, _textures = new ButtonControl("Textures"));
        _singlePlayer.Pressed += OnSinglePlayerPressed;
        _multiplayer.Pressed += OnMultiplayerPressed;
        _options.Enabled = false;
        _mods.Enabled = false;
        _textures.Enabled = false;
    }

    private void OnSinglePlayerPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.StartSinglePlayer("world");
    }

    private void OnMultiplayerPressed()
    {
        Console.WriteLine("Enter server address and port");
        var serverAddressAndPort = Console.ReadLine()!;
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.StartMultiplayer(serverAddressAndPort);
    }

    public override void EscapePressed()
    {
        
    }
}