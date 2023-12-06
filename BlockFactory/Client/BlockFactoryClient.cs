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
using BlockFactory.World_.Interfaces;
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
    public static PlayerEntity Player;
    public static LogicProcessor LogicProcessor;
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
            Samples = 4,
            PreferredDepthBufferBits = 32
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

        if (!MouseInputManager.MouseIsEnabled) Player.HeadRotation -= MouseInputManager.Delta / 100;

        Player.HeadRotation.X %= 2 * MathF.PI;
        Player.HeadRotation.Y = Math.Clamp(Player.HeadRotation.Y, -MathF.PI / 2, MathF.PI / 2);
        PlayerControlState nState = 0;
        if (InputContext.Keyboards[0].IsKeyPressed(Key.W))
        {
            nState |= PlayerControlState.MovingForward;
        }

        if (InputContext.Keyboards[0].IsKeyPressed(Key.S))
        {
            nState |= PlayerControlState.MovingBackwards;
        }

        if (InputContext.Keyboards[0].IsKeyPressed(Key.A))
        {
            nState |= PlayerControlState.MovingLeft;
        }

        if (InputContext.Keyboards[0].IsKeyPressed(Key.D))
        {
            nState |= PlayerControlState.MovingRight;
        }

        if (InputContext.Keyboards[0].IsKeyPressed(Key.Space))
        {
            nState |= PlayerControlState.MovingUp;
        }

        if (InputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft))
        {
            nState |= PlayerControlState.MovingDown;
        }

        if (InputContext.Keyboards[0].IsKeyPressed(Key.ControlLeft))
        {
            nState |= PlayerControlState.Sprinting;
        }

        if (InputContext.Mice[0].IsButtonPressed(MouseButton.Left))
        {
            nState |= PlayerControlState.Attacking;
        }

        if (InputContext.Mice[0].IsButtonPressed(MouseButton.Right))
        {
            nState |= PlayerControlState.Using;
        }

        Player.ControlState = nState;

        var wireframe = InputContext.Keyboards[0].IsKeyPressed(Key.ControlRight);
        if (wireframe) BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        LogicProcessor.Update();
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
        LogicProcessor = new LogicProcessor();
        LogicProcessor.Start();
        WorldRenderer = new WorldRenderer(LogicProcessor.GetWorld());
        Player = new PlayerEntity();
        LogicProcessor.AddPlayer(Player);
        Player.SetWorld(LogicProcessor.GetWorld());
        Player.Pos = new Vector3D<double>(1e7, 0, 0);
    }

    private static void OnWindowClose()
    {
        WorldRenderer.Dispose();
        LogicProcessor.Dispose();

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