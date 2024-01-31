using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using BlockFactory.Base;
using BlockFactory.Client.Gui;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Entity_;
using BlockFactory.Network;
using BlockFactory.Registry_;
using BlockFactory.Resource;
using BlockFactory.Serialization;
using ENet.Managed;
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
    public static PlayerEntity? Player = null;
    public static LogicProcessor? LogicProcessor = null;
    public static WorldRenderer? WorldRenderer = null;
    public static MenuManager MenuManager = null!;

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
            VSync = false,
            PreferredDepthBufferBits = 32
        };
        Window = Silk.NET.Windowing.Window.Create(options);
    }

    private static void UpdateAndRenderInGame(double deltaTime)
    {
        if (!MouseInputManager.MouseIsEnabled) Player!.HeadRotation -= MouseInputManager.Delta / 100;

        Player!.HeadRotation.X %= 2 * MathF.PI;
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

        Player.MotionController.ClientState.ControlState = nState;
        
        LogicProcessor!.Update();
        BfRendering.UseWorldMatrices();
        BfRendering.SetVpMatrices(Shaders.Block);
        Shaders.Block.SetPlayerPos(Vector3D<float>.Zero);
        WorldRenderer!.UpdateAndRender(deltaTime);
        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    private static void UpdateAndRenderHud()
    {
        var size = Window.FramebufferSize;
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(size.X / 2, size.Y / 2 - BfClientContent.TextRenderer.GetStringHeight("X") / 2, 0);
        GuiRenderHelper.RenderText("X", 0);
        BfRendering.Matrices.Pop();
    }

    private static void UpdateAndRender(double deltaTime)
    {
        var wireframe = InputContext.Keyboards[0].IsKeyPressed(Key.ControlRight);
        if (wireframe) BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        MouseInputManager.Update();
        BfRendering.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        BfRendering.Gl.Enable(EnableCap.CullFace);
        BfRendering.Gl.CullFace(TriangleFace.Back);
        BfRendering.Gl.Enable(EnableCap.DepthTest);
        BfRendering.Gl.DepthFunc(DepthFunction.Lequal);

        if (LogicProcessor != null)
        {
            UpdateAndRenderInGame(deltaTime);
        }

        BfRendering.Gl.Clear(ClearBufferMask.DepthBufferBit);
        BfRendering.UseGuiMatrices();

        if (MenuManager.Empty && LogicProcessor == null)
        {
            MenuManager.Push(new MainMenu());
        }
        
        if (MenuManager.Empty && LogicProcessor != null)
        {
            UpdateAndRenderHud();
        }
        else
        {
            MenuManager.UpdateAndRender();
        }
        
        if (wireframe) BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        BfDebug.UpdateAndRender(deltaTime);
    }

    private static IPEndPoint GetEndPoint(string serverAddressAndPort)
    {
        // if (serverAddressAndPort.Length == 0) serverAddressAndPort = Dns.GetHostName();
        if (!serverAddressAndPort.Contains(':')) serverAddressAndPort += ":" + Constants.DefaultPort;
        var a = serverAddressAndPort.Split(':');
        var address = a[0];
        var port = int.Parse(a[1]);
        IPAddress ipAddress;
        if (address == string.Empty)
        {
            ipAddress = IPAddress.Loopback;
        }
        else
        {
            if (!IPAddress.TryParse(address, out ipAddress!))
            {
                var entry = Dns.GetHostEntry(address);
                ipAddress = entry.AddressList.First(addr => addr.AddressFamily == AddressFamily.InterNetwork);
            }
        }

        return new IPEndPoint(ipAddress, port);
    }

    private static void OnWindowLoad()
    {
        ManagedENet.Startup();
        InputContext = Window.CreateInput();
        BfRendering.Init();
        BfDebug.Init();
        BfContent.Init();
        var a = typeof(BlockFactoryClient).Assembly;
        ResourceLoader = new AssemblyResourceLoader(a);
        BfClientContent.Init();
        MenuManager = new MenuManager();
    }

    private static void OnWindowClose()
    {
        if (LogicProcessor != null)
        {
            WorldRenderer?.Dispose();
            LogicProcessor?.Dispose();
            TagIO.Serialize("registry_mapping.dat", SynchronizedRegistries.WriteMapping());
        }

        BfClientContent.Destroy();
        BfDebug.Destroy();
        ManagedENet.Shutdown();
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