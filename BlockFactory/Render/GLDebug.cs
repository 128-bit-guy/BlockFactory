using System.Runtime.InteropServices;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;

namespace BlockFactory.Render;

[ExclusiveTo(Side.Client)]
public class GLDebug
{
    private readonly DebugProc _debugMessageHandler;

    public GLDebug()
    {
        _debugMessageHandler = OnDebugMessage;
    }

    public void Init()
    {
        GL.DebugMessageCallback(_debugMessageHandler, IntPtr.Zero);
    }

    private static void OnDebugMessage(DebugSource source,
        DebugType type,
        int id,
        DebugSeverity severity,
        int length,
        IntPtr message,
        IntPtr userParam)
    {
        string s;
        string msg;
        string typeStr;
        string severityStr;
        switch (type)
        {
            case DebugType.DebugTypeError or DebugType.DebugTypeUndefinedBehavior:
            {
                s = Marshal.PtrToStringAnsi(message, length);
                typeStr = type.ToString()["DebugType".Length..];
                severityStr = severity.ToString()["DebugSeverity".Length..];
                msg = $"{typeStr}: ID - {(ErrorCode)id}, severity - {severityStr}, message:\n{s}";
                throw new GLException(msg);
            }
            case DebugType.DebugTypePerformance or DebugType.DebugTypePortability
                or DebugType.DebugTypeDeprecatedBehavior:
                s = Marshal.PtrToStringAnsi(message, length);
                typeStr = type.ToString()["DebugType".Length..];
                severityStr = severity.ToString()["DebugSeverity".Length..];
                msg = $"{typeStr}: ID - {(ErrorCode)id}, severity - {severityStr}, message:\n{s}";
                Console.WriteLine($"[GL Debug Message] {msg}");
                break;
        }
    }
}