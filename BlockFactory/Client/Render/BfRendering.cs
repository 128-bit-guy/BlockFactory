﻿using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using BlockFactory.Base;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class BfRendering
{
    public static GL Gl = null!;
    public static readonly Color SkyColor = Color.Aqua;
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
    }

    public static void UseWorldMatrices()
    {
        var forward = BlockFactoryClient.CalcCameraForward();
        var up = BlockFactoryClient.CalcCameraUp();
        View = Matrix4X4.CreateLookAt(Vector3D<float>.Zero, forward, up);
        var aspectRatio = (float)BlockFactoryClient.Window.Size.X / BlockFactoryClient.Window.Size.Y;
        Projection = Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 2, aspectRatio, 0.05f,
            300f);
    }

    public static FrustumIntersectionHelper CreateIntersectionHelper()
    {
        return new FrustumIntersectionHelper(View * Projection);
    }

    public static void SetMatrices(ShaderProgram program)
    {
        program.SetView(View);
        program.SetProjection(Projection);
    }
}