using BlockFactory.Client.Render;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui
{
    [ExclusiveTo(Side.Client)]
    public class ForcedDisconnectionScreen : Screen
    {
        public ButtonWidget BackButton = null!;
        public readonly string Reason;
        public ForcedDisconnectionScreen(BlockFactoryClient client, string reason) : base(client)
        {
            int cnt = 0;
            for (int i = 0; i < reason.Length; ++i) {
                if (reason[i] == '\n') {
                    ++cnt;
                }
                if (cnt == 5) {
                    Reason = reason.Substring(0, i) + "\n...";
                    return;
                }
            }
            Reason = reason;
        }

        public ForcedDisconnectionScreen(BlockFactoryClient client, Exception exception) : this(client, exception.GetType().Name + ": " + exception.Message + "\n" + exception.StackTrace)
        {
        }

        public override void InitWidgets(Vector2i size)
        {
            base.InitWidgets(size);
            var (width, height) = size;
            float stringHeight = Textures.TextRenderer.GetStringHeight(Reason);
            float guiHeight = stringHeight + 80;
            float centerX = width / 2f;
            float centerY = height / 2f;
            float guiTop = centerY - guiHeight / 2;
            Widgets.Add(BackButton = new ButtonWidget(this, new(centerX - 300, guiTop + stringHeight + 10, centerX + 300, guiTop + stringHeight + 80), "Back"));
            BackButton.OnClick += (_, _) => Client.PopScreen();
        }

        public override void UpdateAndRender()
        {
            base.UpdateAndRender();
            var (width, height) = Client.GetDimensions();
            float stringHeight = Textures.TextRenderer.GetStringHeight(Reason);
            float guiHeight = stringHeight + 80;
            float centerX = width / 2f;
            float centerY = height / 2f;
            float guiTop = centerY - guiHeight / 2;
            Client.Matrices.Push();
            Client.Matrices.Translate((width / 2f, guiTop, 1));
            DrawText(Reason, 0);
            Client.Matrices.Pop();
        }
    }
}
