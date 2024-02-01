using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class MainMenu : Menu
{
    private readonly ButtonControl SinglePlayer;
    private readonly ButtonControl Multiplayer;
    private readonly ButtonControl Options;
    private readonly ButtonControl Mods;
    private readonly ButtonControl Textures;
    public MainMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(6, 5),
            Array.Empty<int>(), new[]{3})
            .With(0, 0, 5, 0, new LabelControl("Block Factory"))
            .With(0, 1, 5, 1, SinglePlayer = new ButtonControl("Singleplayer"))
            .With(0, 2, 5, 2, Multiplayer = new ButtonControl("Multiplayer"))
            .With(0, 3, 5, 3, Options = new ButtonControl("Options"))
            .With(0, 4, 2, 4, Mods = new ButtonControl("Mods"))
            .With(3, 4, 5, 4, Textures = new ButtonControl("Textures"));
        SinglePlayer.Pressed += OnSinglePlayerPressed;
        Multiplayer.Pressed += OnMultiplayerPressed;
        Options.Enabled = false;
        Mods.Enabled = false;
        Textures.Enabled = false;
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
}