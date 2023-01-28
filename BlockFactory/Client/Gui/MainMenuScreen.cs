using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui
{
    public class MainMenuScreen : Screen
    {
        public ButtonWidget SingleplayerButton = null!;
        public ButtonWidget MultiplayerButton = null!;
        public ButtonWidget SettingsButton = null!;
        public ButtonWidget ResourcePacksButton = null!;
        public ButtonWidget ModsButton = null!;
        public ButtonWidget CreditsButton = null!;
        public ButtonWidget QuitButton = null!;
        public MainMenuScreen(BlockFactoryClient client) : base(client)
        {
        }

        public override void InitWidgets(Vector2i size)
        {
            base.InitWidgets(size);
            var (width, height) = size;
            float centerX = width / 2f;
            float centerY = height / 2f;
            Widgets.Add(SingleplayerButton = new ButtonWidget(this, new(centerX - 300, centerY - 210, centerX + 300, centerY - 140), "Singleplayer"));
            SingleplayerButton.OnClick += (_, _) => StartSingleplayer();
            Widgets.Add(MultiplayerButton = new ButtonWidget(this, new(centerX - 300, centerY - 130, centerX + 300, centerY - 60), "Multiplayer"));
            MultiplayerButton.OnClick += (_, _) => Client.PushScreen(new ServerScreen(Client));
            Widgets.Add(SettingsButton = new ButtonWidget(this, new(centerX - 300, centerY - 50, centerX + 300, centerY + 20), "Settings"));
            SettingsButton.Enabled = false;
            Widgets.Add(ResourcePacksButton = new ButtonWidget(this, new(centerX - 300, centerY + 30, centerX + 300, centerY + 100), "Resource Packs"));
            ResourcePacksButton.Enabled = false;
            Widgets.Add(ModsButton = new ButtonWidget(this, new(centerX - 300, centerY + 110, centerX - 5, centerY + 180), "Mods"));
            ModsButton.Enabled = false;
            Widgets.Add(CreditsButton = new ButtonWidget(this, new(centerX + 5, centerY + 110, centerX + 300, centerY + 180), "Credits"));
            CreditsButton.OnClick += (_, _) => Client.PushScreen(new CreditsScreen(Client));
            Widgets.Add(QuitButton = new ButtonWidget(this, new(centerX - 300, centerY + 190, centerX + 300, centerY + 260), "Quit"));
            QuitButton.OnClick += (_, _) => Client.Stop();
        }

        private void StartSingleplayer() {
            Client.InitSingleplayerGameInstance();
            while (Client.HasScreen())
            {
                Client.PopScreen();
            }
        }

        public override void UpdateAndRender()
        {
            base.UpdateAndRender();
            var (width, height) = Client.GetDimensions();
            Client.Matrices.Push();
            Client.Matrices.Translate((width / 2f, height / 2 - 360, 1));
            Client.Matrices.Scale(2);
            DrawText("Voxel Builder", 0);
            Client.Matrices.Pop();
        }
    }
}
