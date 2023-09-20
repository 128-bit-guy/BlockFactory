using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using BlockFactory.Base;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;


[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class BfRendering
{
    public static GL Gl = null!;

    public static unsafe void OnWindowLoad()
    {
        Gl = BlockFactoryClient.Window.CreateOpenGL();
        Gl.ClearColor(Color.Aqua);
        DebugProc d;
        Gl.DebugMessageCallback(OnDebugMessage, null);
        Gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
    }

    private static void OnDebugMessage(GLEnum source,
        GLEnum type,
        int id,
        GLEnum severity,
        int length,
        IntPtr message,
        IntPtr userParam)
    {
        if (type != GLEnum.DebugTypeError && type != GLEnum.DebugTypeUndefinedBehavior && type != GLEnum.DebugTypeDeprecatedBehavior) return;
        var s = Marshal.PtrToStringAnsi(message, length);
        throw new GlException($"Error {(ErrorCode)id} from {source} with severity {severity} and message:\n{s}");
    }

    public static void OnFramebufferResize(Vector2D<int> newSize)
    {
        Gl.Viewport(newSize);
    }
}