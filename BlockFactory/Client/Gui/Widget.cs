using BlockFactory.Side_;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Gui
{
    [ExclusiveTo(Side.Client)]
    public class Widget : IDisposable
    {
        protected readonly Screen Screen;
        public readonly Box2 Box;
        public float ZIndex;

        public Widget(Screen screen, Box2 box)
        {
            Screen = screen;
            Box = box;
            ZIndex = 1;
        }

        public unsafe bool IsMouseOver()
        {
            GLFW.GetCursorPos(Screen.Client.Window, out var x, out var y);
            return Box.Contains(((float)x, (float)y));
        }

        public virtual void UpdateAndRender() { 
            
        }

        public virtual void Dispose()
        {

            GC.SuppressFinalize(this);
        }
    }
}
