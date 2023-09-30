using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.CubeMath;
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
        BfRendering.Gl.Enable(EnableCap.DepthTest);
        BfRendering.Gl.DepthFunc(DepthFunction.Lequal);
        var builder = new MeshBuilder<BlockVertex>();
        builder.Matrices.Push();
        foreach (var face in CubeFaceUtils.Values())
        {
            builder.Matrices.Push();
            CubeSymmetry s;
            if (face.GetAxis() == 1)
            {
                s = CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0];
            }
            else
            {
                s = CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;
            }
            builder.Matrices.Multiply(s.Matrix4);
            var light = (float)(face.GetAxis() + 8) / 10;
            var light2 = light / 2;
            builder.NewPolygon().Indices(0, 1, 2, 0, 2, 3)
                .Vertex(new BlockVertex(-1, -1, -1, light, light, light, 1, 0, 0))
                .Vertex(new BlockVertex(-1, 1, -1, light, light, light, 1, 0, 1))
                .Vertex(new BlockVertex(1, 1, -1, light, light, light, 1, 1, 1))
                .Vertex(new BlockVertex(1, -1, -1, light, light, light, 1, 1, 0));
            builder.Matrices.Push();
            builder.Matrices.Multiply(Matrix4X4.CreateRotationZ((float)Window.Time));
            builder.NewPolygon().Indices(0, 1, 2, 0, 2, 3)
                .Vertex(new BlockVertex(0, 0, -2, light2, light, light2, 1, 0, 0))
                .Vertex(new BlockVertex(0, 2, -2, light2, light2, light, 1, 0, 1))
                .Vertex(new BlockVertex(2, 2, -2, light2, light, light2, 1, 1, 1))
                .Vertex(new BlockVertex(2, 0, -2, light2, light2, light, 1, 1, 0));
            builder.Matrices.Pop();
            builder.Matrices.Pop();
        }
        builder.Matrices.Pop();
        builder.Upload(_triangle);
        builder.Reset();
        _program.SetModel(Matrix4X4<float>.Identity);
        var cameraPos =
            new Vector3D<float>((float)Math.Sin(Window.Time * 0.75), (float)Math.Cos(Window.Time * 1.25) / 3,
                (float)Math.Cos(Window.Time * 0.75)) * 10;
        _program.SetView(Matrix4X4.CreateLookAt(cameraPos, Vector3D<float>.Zero, 
            Vector3D<float>.UnitY));
        var aspectRatio = (float)Window.Size.X / Window.Size.Y;
        _program.SetProjection(Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 2, aspectRatio, 0.05f,
            100f));
        _program.Use();
        _triangle.Bind();
        Textures.Stone.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _triangle.IndexCount, DrawElementsType.UnsignedInt,
            null);
        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
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
        Textures.Init();
    }

    private static void OnWindowClose()
    {
        Textures.Destroy();
        _program.Dispose();
        _triangle.Dispose();
        BfDebug.Destroy();
    }

    private static void AddEvents()
    {
        Window.Load += BfRendering.Init;
        Window.Load += BfDebug.Init;
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