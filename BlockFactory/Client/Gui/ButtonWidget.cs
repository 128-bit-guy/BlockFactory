using BlockFactory.Client.Render;
using BlockFactory.Side_;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Gui
{
    [ExclusiveTo(Side.Client)]
    public class ButtonWidget : Widget
    {
        public readonly string Text;
        public delegate void ClickHandler(MouseButton button, KeyModifiers modifiers);
        public event ClickHandler OnClick;
        public bool Enabled;
        public ButtonWidget(Screen screen, Box2 box, string text) : base(screen, box)
        {
            Text = text;
            OnClick = (_, _) => { };
            Enabled = true;
            screen.Client.OnMouseButton += OnMouseButton;
        }

        public override void UpdateAndRender()
        {
            base.UpdateAndRender();
            bool mouseOver = IsMouseOver();
            if (!Enabled)
            {
                Screen.GuiMeshBuilder.Color = (0.5f, 0.5f, 0.5f);
            }
            else if (mouseOver)
            {
                Screen.GuiMeshBuilder.Color = (0.8f, 0.8f, 1f);
            }
            Screen.DrawTexturedRect(Box, ZIndex, 64, Textures.StoneTexture);
            Screen.Client.Matrices.Push();
            Screen.Client.Matrices.Translate(new Vector3(Box.Center.X, Box.Center.Y - Textures.TextRenderer.GetStringHeight(Text) / 2 - 3, ZIndex + 0.5f));
            if (!Enabled)
            {
                Screen.GuiMeshBuilder.Color = (0.5f, 0.5f, 0.5f);
            }
            else if (mouseOver)
            {
                Screen.GuiMeshBuilder.Color = (1f, 1f, 0.8f);
            }
            Screen.DrawText(Text, 0);
            Screen.Client.Matrices.Pop();
            Screen.DrawColoredRect(new(Box.Min, (Box.Max.X, Box.Min.Y + 5)), ZIndex + 0.5f, (0, 0, 0, 1));
            Screen.DrawColoredRect(new(Box.Min, (Box.Min.X + 5, Box.Max.Y)), ZIndex + 0.5f, (0, 0, 0, 1));
            Screen.DrawColoredRect(new((Box.Min.X, Box.Max.Y - 5), Box.Max), ZIndex + 0.5f, (0, 0, 0, 1));
            Screen.DrawColoredRect(new((Box.Max.X - 5, Box.Min.Y), Box.Max), ZIndex + 0.5f, (0, 0, 0, 1));
        }

        public override void Dispose()
        {
            Screen.Client.OnMouseButton -= OnMouseButton;
            base.Dispose();
        }

        private void OnMouseButton(MouseButton button, InputAction action, KeyModifiers modifiers)
        {
            if (Enabled && action == InputAction.Press && IsMouseOver())
            {
                OnClick(button, modifiers);
            }
        }
    }
}
