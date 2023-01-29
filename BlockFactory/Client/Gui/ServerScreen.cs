using OpenTK.Mathematics;
using System.Net.Sockets;
using BlockFactory.Side_;

namespace BlockFactory.Client.Gui
{
    [ExclusiveTo(Side.Client)]
    public class ServerScreen : Screen
    {
        public ButtonWidget BackButton = null!;
        public ButtonWidget ConnectButton = null!;
        public TextInputWidget TextInput = null!;

        public ServerScreen(BlockFactoryClient client) : base(client)
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
            Widgets.Add(ConnectButton = new ButtonWidget(this, new(centerX - 300, centerY + 110, centerX + 300, centerY + 180), "Connect"));
            ConnectButton.OnClick += (_, _) => Connect();
            Widgets.Add(TextInput = new TextInputWidget(this, new(centerX - 300, centerY + 30, centerX + 300, centerY + 100)));
            TextInput.OnEnterPressed += Connect;
        }

        private void Connect() {
            try
            {
                Client.InitMultiplayerGameInstance(TextInput.Text);
            }
            catch (SocketException ex) {
                Client.CleanupGameInstance();
                Client.PopScreen();
                Client.PushScreen(new ForcedDisconnectionScreen(Client, ex));
                return;
            }
            while (Client.HasScreen()) {
                Client.PopScreen();
            }
        }

        public override void UpdateAndRender()
        {
            base.UpdateAndRender();
            var (width, height) = Client.GetDimensions();
            Client.Matrices.Push();
            Client.Matrices.Translate((width / 2f, height / 2 - 240, 1));
            DrawText("Multiplayer", 0);
            Client.Matrices.Pop();
        }
    }
}
