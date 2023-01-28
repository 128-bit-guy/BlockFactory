using BlockFactory.Client.Render;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Gui
{
    public class TextInputWidget : Widget
    {
        public string Text = "";
        public int CursorPos = 0;
        public event Action OnEnterPressed = () => { };
        public TextInputWidget(Screen screen, Box2 box) : base(screen, box)
        {
            Screen.Client.OnMouseButton += OnMouseButton;
            Screen.Client.OnCharInput += OnCharInput;
            Screen.Client.OnKeyInput += OnKeyInput;
        }

        public unsafe bool IsMouseOver()
        {
            GLFW.GetCursorPos(Screen.Client.Window, out var x, out var y);
            return Box.Contains(((float)x, (float)y));
        }

        public override void UpdateAndRender()
        {
            base.UpdateAndRender();
            Screen.DrawColoredRect(Box, ZIndex, (0, 0, 0, 1));
            string textToRender = GetRenderText();
            Screen.Client.Matrices.Push();
            Screen.Client.Matrices.Translate(new Vector3(Box.Min.X + 10, Box.Center.Y - Textures.TextRenderer.GetStringHeight(textToRender) / 2 - 3, ZIndex + 0.5f));
            Screen.DrawText(textToRender, -1);
            Screen.Client.Matrices.Pop();
            Screen.DrawColoredRect(new(Box.Min, (Box.Max.X, Box.Min.Y + 5)), ZIndex + 0.5f, (0.8f, 0.8f, 0.8f, 1));
            Screen.DrawColoredRect(new(Box.Min, (Box.Min.X + 5, Box.Max.Y)), ZIndex + 0.5f, (0.8f, 0.8f, 0.8f, 1));
            Screen.DrawColoredRect(new((Box.Min.X, Box.Max.Y - 5), Box.Max), ZIndex + 0.5f, (0.8f, 0.8f, 0.8f, 1));
            Screen.DrawColoredRect(new((Box.Max.X - 5, Box.Min.Y), Box.Max), ZIndex + 0.5f, (0.8f, 0.8f, 0.8f, 1));
        }

        private bool ShouldAddCursor()
        {
            if (Screen.ActiveWidget == this)
            {
                if (CursorPos == Text.Length)
                {
                    return DateTime.UtcNow.Millisecond >= 500;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private string GetRenderText()
        {
            if (ShouldAddCursor())
            {
                return Text.Insert(CursorPos, CursorPos == Text.Length ? "_" : "|");
            }
            else
            {
                return Text;
            }
        }

        public override void Dispose()
        {
            Screen.Client.OnMouseButton -= OnMouseButton;
            Screen.Client.OnCharInput -= OnCharInput;
            Screen.Client.OnKeyInput -= OnKeyInput;
            base.Dispose();
        }

        private void OnMouseButton(MouseButton button, InputAction action, KeyModifiers modifiers)
        {
            if (action == InputAction.Press)
            {
                if (IsMouseOver())
                {
                    Screen.ActiveWidget = this;
                }
                else if (Screen.ActiveWidget == this)
                {
                    Screen.ActiveWidget = null;
                }
            }
        }

        private void OnCharInput(string s)
        {
            if (Screen.ActiveWidget == this)
            {
                Text = Text.Insert(CursorPos, s);
                CursorPos += s.Length;
            }
        }

        private void OnKeyInput(Keys keys, int scancode, InputAction action, KeyModifiers mods)
        {
            if (Screen.ActiveWidget == this && action != InputAction.Release)
            {
                switch (keys)
                {
                    case Keys.Escape:
                        Screen.ActiveWidget = null;
                        break;
                    case Keys.Backspace:
                        if (CursorPos != 0)
                        {
                            Text = Text.Remove(CursorPos - 1, 1);
                            --CursorPos;
                        }
                        break;
                    case Keys.Left:
                        if (CursorPos != 0)
                        {
                            --CursorPos;
                        }
                        break;
                    case Keys.Right:
                        if (CursorPos != Text.Length)
                        {
                            ++CursorPos;
                        }
                        break;
                    case Keys.Delete:
                        if (CursorPos != Text.Length)
                        {
                            Text = Text.Remove(CursorPos, 1);
                        }
                        break;
                    case Keys.Enter:
                        OnEnterPressed();
                        break;
                }
            }
        }
    }
}
