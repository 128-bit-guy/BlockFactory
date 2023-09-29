using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Resource;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class BlockFactoryClient
{
    public static IWindow Window = null!;
    public static ResourceLoader ResourceLoader = null!;
    private static ShaderProgram _program;

    private static RenderMesh _triangle;

    private static void InitWindow()
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = Constants.Name,
            ShouldSwapAutomatically = true,
            API = GraphicsAPI.Default with
            {
                Flags = ContextFlags.Debug | ContextFlags.ForwardCompatible
            },
            Samples = 4
        };
        Window = Silk.NET.Windowing.Window.Create(options);
    }
    private static unsafe void UpdateAndRender(double deltaTime)
    {
        BfRendering.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        BfRendering.Gl.Enable(EnableCap.CullFace);
        BfRendering.Gl.CullFace(TriangleFace.Back);
        var builder = new MeshBuilder<BlockVertex>();
        builder.Matrices.Push();
        builder.NewPolygon().Indices(0, 1, 2)
            .Vertex(new BlockVertex(0, 1, 0))
            .Vertex(new BlockVertex(1, 0, 0))
            .Vertex(new BlockVertex(-1, 0, 0));
        builder.NewPolygon().Indices(0, 1, 2)
            .Vertex(new BlockVertex((float)Math.Sin(Window.Time), 0, 0))
            .Vertex(new BlockVertex(1, -1, 0))
            .Vertex(new BlockVertex(-1, -1, 0));
        builder.Matrices.Pop();
        builder.Upload(_triangle);
        builder.Reset();
        _program.SetModel(Matrix4X4<float>.Identity);
        var cameraPos =
            new Vector3D<float>((float)Math.Sin(Window.Time * 0.75), (float)Math.Cos(Window.Time * 1.25) / 5,
                (float)Math.Cos(Window.Time * 0.75)) * 10;
        _program.SetView(Matrix4X4.CreateLookAt(cameraPos, Vector3D<float>.Zero, 
            Vector3D<float>.UnitY));
        var aspectRatio = (float)Window.Size.X / Window.Size.Y;
        _program.SetProjection(Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 2, aspectRatio, 0.05f,
            100f));
        _program.Use();
        _triangle.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _triangle.IndexCount, DrawElementsType.UnsignedInt,
            null);
        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfDebug.UpdateAndRender(deltaTime);
    }

    private static void OnWindowLoad()
    {
        var a = typeof(BlockFactoryClient).Assembly;
        ResourceLoader = new AssemblyResourceLoader(a);
        var vertText = ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Vertex.glsl")!;
        var fragText = ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Fragment.glsl")!;
        _program = new ShaderProgram(vertText, fragText);
        _triangle = new RenderMesh(VertexBufferObjectUsage.StreamDraw);
    }

    private static void OnWindowClose()
    {
        _program.Dispose();
        _triangle.Dispose();
        BfDebug.OnWindowClose();
    }

    private static void AddEvents()
    {
        Window.Load += BfRendering.OnWindowLoad;
        Window.Load += BfDebug.OnWindowLoad;
        Window.Load += OnWindowLoad;
        Window.Render += UpdateAndRender;
        Window.FramebufferResize += BfRendering.OnFramebufferResize;
        Window.Closing += OnWindowClose;
    }

    public static void Run()
    {
        InitWindow();
        AddEvents();
        Window.Run();
    }
    
}