using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Resource;
using BlockFactory.World_;
using ImGuiNET;
using Silk.NET.Input;
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
    public static IInputContext InputContext = null!;
    private static RenderMesh _triangle;
    private static Vector2D<float> _headRotation;
    private static Vector3D<float> _cameraPos;

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
        MouseInputManager.Update();
        BfRendering.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        BfRendering.Gl.Enable(EnableCap.CullFace);
        BfRendering.Gl.CullFace(TriangleFace.Back);
        BfRendering.Gl.Enable(EnableCap.DepthTest);
        BfRendering.Gl.DepthFunc(DepthFunction.Lequal);
        _program.SetModel(Matrix4X4<float>.Identity);
        
        if (!MouseInputManager.MouseIsEnabled)
        {
            _headRotation -= MouseInputManager.Delta / 100;
        }

        _headRotation.X %= 2 * MathF.PI;
        _headRotation.Y = Math.Clamp(_headRotation.Y, -MathF.PI / 2, MathF.PI / 2);
        var forward = new Vector3D<float>(MathF.Sin(_headRotation.X) * MathF.Cos(_headRotation.Y), MathF.Sin(_headRotation.Y),
            MathF.Cos(_headRotation.X) * MathF.Cos(_headRotation.Y));
        var up = new Vector3D<float>(MathF.Sin(_headRotation.X) * MathF.Cos(_headRotation.Y + MathF.PI / 2),
            MathF.Sin(_headRotation.Y + MathF.PI / 2),
            MathF.Cos(_headRotation.X) * MathF.Cos(_headRotation.Y + MathF.PI / 2));
        Vector3D<float> moveDelta = new Vector3D<float>();
        if (InputContext.Keyboards[0].IsKeyPressed(Key.W))
        {
            moveDelta += forward;
        }
        if (InputContext.Keyboards[0].IsKeyPressed(Key.S))
        {
            moveDelta -= forward;
        }
        if (InputContext.Keyboards[0].IsKeyPressed(Key.Space))
        {
            moveDelta += Vector3D<float>.UnitY;
        }
        if (InputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft))
        {
            moveDelta -= Vector3D<float>.UnitY;
        }

        var wireframe = InputContext.Keyboards[0].IsKeyPressed(Key.ControlLeft);
        if (wireframe)
        {
            BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        }
        _cameraPos += moveDelta * (float)deltaTime * 5;
        _program.SetView(Matrix4X4.CreateLookAt(_cameraPos, _cameraPos + forward, up));
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
        if (wireframe)
        {
            BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }
        BfDebug.UpdateAndRender(deltaTime);
    }

    private static void OnWindowLoad()
    {
        InputContext = Window.CreateInput();
        BfRendering.Init();
        BfDebug.Init();
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
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            if (i == 0 && j == 0 && k == 0) continue;
            c.Neighbourhood.AddChunk(
                new Chunk(new Vector3D<int>(i, j, k)) { Data = new ChunkData() });
        }

        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
            c.SetBlock(new Vector3D<int>(i, 0, j), 1);

        for (var j = 1; j < 4; ++j)
        {
            for (var i = 2; i < 14; ++i)
            {
                c.SetBlock(new Vector3D<int>(1, j, i), 3);
                c.SetBlock(new Vector3D<int>(14, j, i), 3);
                c.SetBlock(new Vector3D<int>(i, j, 1), 3);
                c.SetBlock(new Vector3D<int>(i, j, 14), 3);
            }

            c.SetBlock(new Vector3D<int>(1, j, 1), 4);
            c.SetBlock(new Vector3D<int>(14, j, 1), 4);
            c.SetBlock(new Vector3D<int>(14, j, 14), 4);
            c.SetBlock(new Vector3D<int>(1, j, 14), 4);
        }

        c.SetBlock(new Vector3D<int>(1, 1, 7), 0);
        c.SetBlock(new Vector3D<int>(1, 2, 7), 0);
        c.SetBlock(new Vector3D<int>(1, 1, 8), 0);
        c.SetBlock(new Vector3D<int>(1, 2, 8), 0);

        for (var i = 1; i < 15; ++i)
        for (var j = 1; j < 15; ++j)
            c.SetBlock(new Vector3D<int>(i, 4, j), 2);

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