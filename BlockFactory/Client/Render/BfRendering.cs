using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using BlockFactory.Base;
using BlockFactory.Utils;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class BfRendering
{
    public static GL Gl = null!;
    public static readonly Color SkyColor = Color.Aqua;
    public static MatrixStack Matrices = new();
    public static Matrix4X4<float> View { get; private set; }
    public static Matrix4X4<float> Projection { get; private set; }

    public static unsafe void Init()
    {
        Gl = BlockFactoryClient.Window.CreateOpenGL();
        Gl.ClearColor(SkyColor);
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
        if (type != GLEnum.DebugTypeError && type != GLEnum.DebugTypeUndefinedBehavior &&
            type != GLEnum.DebugTypeDeprecatedBehavior) return;
        var s = Marshal.PtrToStringAnsi(message, length);
        throw new GlException($"Error {(ErrorCode)id} from {source} with severity {severity} and message:\n{s}");
    }

    public static void OnFramebufferResize(Vector2D<int> newSize)
    {
        Gl.Viewport(newSize);
        BlockFactoryClient.SkyRenderer?.OnFramebufferResize(newSize);
    }

    public static void UseWorldMatrices()
    {
        var forward = BlockFactoryClient.Player.GetViewForward();
        var up = BlockFactoryClient.Player.GetViewUp();
        View = Matrix4X4.CreateLookAt(Vector3D<float>.Zero, forward, up);
        var aspectRatio = (float)BlockFactoryClient.Window.Size.X / BlockFactoryClient.Window.Size.Y;
        Projection = CreatePerspective();
    }

    public static void UseGuiMatrices()
    {
        var size = BlockFactoryClient.Window.FramebufferSize;
        View = Matrix4X4<float>.Identity;
        Projection = Matrix4X4.CreateOrthographicOffCenter<float>(0, size.X, size.Y, 0, -100,
            100);
    }

    public static void Use3DHudMatrices()
    {
        View = Matrix4X4<float>.Identity;
        var aspectRatio = (float)BlockFactoryClient.Window.Size.X / BlockFactoryClient.Window.Size.Y;
        Projection = CreatePerspective();
    }

    public static Matrix4X4<float> CreatePerspective()
    {
        var aspectRatio = (float)BlockFactoryClient.Window.Size.X / BlockFactoryClient.Window.Size.Y;
        return Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 2, aspectRatio, 0.05f,
            300f);
    }

    public static Vector3D<float> GetDirectionFromPosFor3DHud(Vector2D<float> pos)
    {
        Matrix4X4.Invert(CreatePerspective(), out var inv);
        pos.Y = -pos.Y;
        return Vector3D.Normalize(BfMathUtils.Unproject(
            new Vector3D<float>(pos, 0), -1, -1, 2, 2, -1, 1, inv));
    }

    public static FrustumIntersectionHelper CreateIntersectionHelper()
    {
        return new FrustumIntersectionHelper(View * Projection);
    }

    public static void SetVpMatrices(ShaderProgram program)
    {
        program.SetView(View);
        program.SetProjection(Projection);
    }
}