using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.CubeMath;
using BlockFactory.Resource;
using BlockFactory.World_;
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
    private static MeshBuilder<BlockVertex> _meshBuilder;
    private static TextureAtlasUvTransformer _uvTransformer;
    private static uint[] _quadIndices = { 0, 1, 2, 0, 2, 3 };

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
        Textures.Blocks.Bind();
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
        _uvTransformer = new TextureAtlasUvTransformer(Textures.Blocks);
        _meshBuilder = new MeshBuilder<BlockVertex>(_uvTransformer);
        var builder = _meshBuilder;
        var c = new Chunk(new Vector3D<int>(0, 0, 0));
        c.Data = new ChunkData();
        for (var i = -1; i <= 1; ++i)
        {
            for (var j = -1; j <= 1; ++j)
            {
                for (var k = -1; k <= 1; ++k)
                {
                    if(i == 0 && j == 0 && k == 0) continue;
                    c.Neighbourhood.AddChunk(
                        new Chunk(new Vector3D<int>(i, j, k)) {Data = new ChunkData()});
                }
            }
        }
        c.SetBlock(new Vector3D<int>(0, 0, 0), 1);
        c.SetBlock(new Vector3D<int>(0, 1, 0), 1);
        c.SetBlock(new Vector3D<int>(0, 2, 0), 2);
        var cr = new ChunkRenderer(c);
        cr.BuildChunkMesh(builder, _uvTransformer);
        builder.Upload(_triangle);
        builder.Reset();
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