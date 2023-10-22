using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Entity_;
using BlockFactory.Registry_;
using BlockFactory.Resource;
using BlockFactory.Serialization;
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
    public static Vector2D<float> HeadRotation;
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

    public static Vector3D<float> CalcCameraForward()
    {
        return new Vector3D<float>(MathF.Sin(HeadRotation.X) * MathF.Cos(HeadRotation.Y),
            MathF.Sin(HeadRotation.Y),
            MathF.Cos(HeadRotation.X) * MathF.Cos(HeadRotation.Y));
    }

    public static Vector3D<float> CalcCameraUp()
    {
        return new Vector3D<float>(MathF.Sin(HeadRotation.X) * MathF.Cos(HeadRotation.Y + MathF.PI / 2),
            MathF.Sin(HeadRotation.Y + MathF.PI / 2),
            MathF.Cos(HeadRotation.X) * MathF.Cos(HeadRotation.Y + MathF.PI / 2));
    }

    private static void UpdateAndRender(double deltaTime)
    {
        MouseInputManager.Update();
        BfRendering.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        BfRendering.Gl.Enable(EnableCap.CullFace);
        BfRendering.Gl.CullFace(TriangleFace.Back);
        BfRendering.Gl.Enable(EnableCap.DepthTest);
        BfRendering.Gl.DepthFunc(DepthFunction.Lequal);

        if (!MouseInputManager.MouseIsEnabled) HeadRotation -= MouseInputManager.Delta / 100;

        HeadRotation.X %= 2 * MathF.PI;
        HeadRotation.Y = Math.Clamp(HeadRotation.Y, -MathF.PI / 2, MathF.PI / 2);
        var forward = CalcCameraForward();
        var moveDelta = new Vector3D<float>();
        if (InputContext.Keyboards[0].IsKeyPressed(Key.W)) moveDelta += forward;
        if (InputContext.Keyboards[0].IsKeyPressed(Key.S)) moveDelta -= forward;
        if (InputContext.Keyboards[0].IsKeyPressed(Key.Space)) moveDelta += Vector3D<float>.UnitY;
        if (InputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft)) moveDelta -= Vector3D<float>.UnitY;

        var blockPos =
            new Vector3D<double>(Math.Floor(Player.Pos.X), Math.Floor(Player.Pos.Y), Math.Floor(Player.Pos.Z))
                .As<int>();

        if (InputContext.Mice[0].IsButtonPressed(MouseButton.Left)) _world.SetBlock(blockPos, 0);
        if (InputContext.Mice[0].IsButtonPressed(MouseButton.Right)) _world.SetBlock(blockPos, Blocks.Bricks);

        var wireframe = InputContext.Keyboards[0].IsKeyPressed(Key.ControlLeft);
        if (wireframe) BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        Player.Pos += (moveDelta * (float)deltaTime * 5).As<double>();
        Player.Update();
        _world.Update();
        BfRendering.UseWorldMatrices();
        BfRendering.SetMatrices(Shaders.Block);
        Shaders.Block.SetPlayerPos(Vector3D<float>.Zero);
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
        BfContent.Init();
        var a = typeof(BlockFactoryClient).Assembly;
        ResourceLoader = new AssemblyResourceLoader(a);
        Textures.Init();
        Shaders.Init();
        var mapping = new RegistryMapping();
        if (File.Exists("registry_mapping.dat"))
        {
            TagIO.Deserialize("registry_mapping.dat", mapping);
        }
        SynchronizedRegistries.LoadMapping(mapping);
        _world = new World("world");
        WorldRenderer = new WorldRenderer(_world);
        Player = new PlayerEntity();
        Player.SetWorld(_world);
        Player.Pos = new Vector3D<double>(1e7, 0, 0);
    }

    private static void OnWindowClose()
    {
        WorldRenderer.Dispose();
        _world.Dispose();
        
        TagIO.Serialize("registry_mapping.dat", SynchronizedRegistries.WriteMapping());
        Textures.Destroy();
        Shaders.Destroy();
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