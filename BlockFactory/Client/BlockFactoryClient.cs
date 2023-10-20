using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Entity_;
using BlockFactory.Resource;
using BlockFactory.World_;
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
    public static IInputContext InputContext = null!;
    private static Vector2D<float> _headRotation;
    public static PlayerEntity Player;
    private static World _world;
    public static WorldRenderer WorldRenderer;

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

    private static void UpdateAndRender(double deltaTime)
    {
        MouseInputManager.Update();
        BfRendering.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        BfRendering.Gl.Enable(EnableCap.CullFace);
        BfRendering.Gl.CullFace(TriangleFace.Back);
        BfRendering.Gl.Enable(EnableCap.DepthTest);
        BfRendering.Gl.DepthFunc(DepthFunction.Lequal);

        if (!MouseInputManager.MouseIsEnabled) _headRotation -= MouseInputManager.Delta / 100;

        _headRotation.X %= 2 * MathF.PI;
        _headRotation.Y = Math.Clamp(_headRotation.Y, -MathF.PI / 2, MathF.PI / 2);
        var forward = new Vector3D<float>(MathF.Sin(_headRotation.X) * MathF.Cos(_headRotation.Y),
            MathF.Sin(_headRotation.Y),
            MathF.Cos(_headRotation.X) * MathF.Cos(_headRotation.Y));
        var up = new Vector3D<float>(MathF.Sin(_headRotation.X) * MathF.Cos(_headRotation.Y + MathF.PI / 2),
            MathF.Sin(_headRotation.Y + MathF.PI / 2),
            MathF.Cos(_headRotation.X) * MathF.Cos(_headRotation.Y + MathF.PI / 2));
        var moveDelta = new Vector3D<float>();
        if (InputContext.Keyboards[0].IsKeyPressed(Key.W)) moveDelta += forward;
        if (InputContext.Keyboards[0].IsKeyPressed(Key.S)) moveDelta -= forward;
        if (InputContext.Keyboards[0].IsKeyPressed(Key.Space)) moveDelta += Vector3D<float>.UnitY;
        if (InputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft)) moveDelta -= Vector3D<float>.UnitY;

        var blockPos =
            new Vector3D<double>(Math.Floor(Player.Pos.X), Math.Floor(Player.Pos.Y), Math.Floor(Player.Pos.Z))
                .As<int>();

        if (InputContext.Mice[0].IsButtonPressed(MouseButton.Left)) _world.SetBlock(blockPos, 0);
        if (InputContext.Mice[0].IsButtonPressed(MouseButton.Right)) _world.SetBlock(blockPos, 3);

        var wireframe = InputContext.Keyboards[0].IsKeyPressed(Key.ControlLeft);
        if (wireframe) BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        Player.Pos += (moveDelta * (float)deltaTime * 5).As<double>();
        Player.Update();
        _world.Update();
        Shaders.Block.SetView(Matrix4X4.CreateLookAt(Player.Pos.As<float>(),
            Player.Pos.As<float>() + forward, up));
        var aspectRatio = (float)Window.Size.X / Window.Size.Y;
        Shaders.Block.SetProjection(Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 2, aspectRatio, 0.05f,
            300f));
        Shaders.Block.SetPlayerPos(Player.Pos.As<float>());
        WorldRenderer.UpdateAndRender(deltaTime);
        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
        if (wireframe) BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        BfDebug.UpdateAndRender(deltaTime);
    }

    private static void OnWindowLoad()
    {
        InputContext = Window.CreateInput();
        BfRendering.Init();
        BfDebug.Init();
        var a = typeof(BlockFactoryClient).Assembly;
        ResourceLoader = new AssemblyResourceLoader(a);
        Textures.Init();
        Shaders.Init();
        _world = new World();
        WorldRenderer = new WorldRenderer(_world);
        Player = new PlayerEntity();
        Player.SetWorld(_world);
    }

    private static void OnWindowClose()
    {
        Textures.Destroy();
        Shaders.Destroy();
        WorldRenderer.Dispose();
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