using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui
{
    public class PauseMenuScreen : Screen
    {
        public ButtonWidget BackButton = null!;
        public ButtonWidget DisconnectButton = null!;
        public ButtonWidget SettingsButton = null!;
        public ButtonWidget ResourcePacksButton = null!;
        public ButtonWidget ModsButton = null!;
        public PauseMenuScreen(BlockFactoryClient client) : base(client)
        {
        }

        public override void InitWidgets(Vector2i size)
        {
            base.InitWidgets(size);
            var (width, height) = size;
            float centerX = width / 2f;
            float centerY = height / 2f;
            Widgets.Add(BackButton = new ButtonWidget(this, new(centerX - 300, centerY + 190, centerX + 300, centerY + 260), "Back"));
            BackButton.OnClick += (_, _) => Client.PopScreen();
            Widgets.Add(DisconnectButton = new ButtonWidget(this, new(centerX - 300, centerY + 110, centerX + 300, centerY + 180), "Disconnect"));
            DisconnectButton.OnClick += (_, _) => Disconnect();
            Widgets.Add(SettingsButton = new ButtonWidget(this, new(centerX - 300, centerY - 130, centerX + 300, centerY - 60), "Settings"));
            SettingsButton.Enabled = false;
            Widgets.Add(ResourcePacksButton = new ButtonWidget(this, new(centerX - 300, centerY - 50, centerX + 300, centerY + 20), "Resource Packs"));
            ResourcePacksButton.Enabled = false;
            Widgets.Add(ModsButton = new ButtonWidget(this, new(centerX - 300, centerY + 30, centerX + 300, centerY + 100), "Mods"));
            ModsButton.Enabled = false;
        }

        private void Disconnect() {
            Client.PopScreen();
            Client.CleanupGameInstance();
            Client.PushScreen(new MainMenuScreen(Client));
        }

        public override void UpdateAndRender()
        {
            base.UpdateAndRender();
            var (width, height) = Client.GetDimensions();
            Client.Matrices.Push();
            Client.Matrices.Translate((width / 2f, height / 2 - 240, 1));
            DrawText("Ingame Menu", 0);
            Client.Matrices.Pop();
        }
    }
}
