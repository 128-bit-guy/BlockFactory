using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui
{
    public class CreditsScreen : Screen
    {
        public ButtonWidget BackButton = null!;
        public CreditsScreen(BlockFactoryClient client) : base(client)
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
            Client.Matrices.Push();
            Client.Matrices.Translate((width / 2f, height / 2 - 240, 1));
            DrawText("Author - Waley\nTextures - TextureCan (https://www.texturecan.com/)\nLibraries - OpenTK, ZstdSharp,\nRecyclableMemoryStream,\nStbImageSharp, StbTrueTypeSharp", 0);
            Client.Matrices.Pop();
        }
    }
}
