using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using BlockFactory.Base;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;


[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class BfRendering
{
    public static GL Gl = null!;

    public static void OnWindowLoad()
    {
        Gl = BlockFactoryClient.Window.CreateOpenGL();
        Gl.ClearColor(Color.Aqua);
    }

    public static void OnFramebufferResize(Vector2D<int> newSize)
    {
        Gl.Viewport(newSize);
    }
}