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
    public static string WorldsDirectory = null!;

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
        PlayerControlManager.Update(deltaTime);
        Player.MotionController.ClientState.ControlState = PlayerControlManager.ControlState;

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
        BfRendering.Matrices.Translate(size.X / 2, size.Y / 2 - BfClientContent.TextRenderer.GetStringHeight("X") / 2,
            0);
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

        if (!MenuManager.HasAnythingToRender() && LogicProcessor != null)
        {
            UpdateAndRenderHud();
        }
        else
        {
            MenuManager.UpdateAndRender(deltaTime);
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

    public static void StartSinglePlayer(string saveName, WorldSettings? settings = null)
    {
        LogicProcessor =
            new LogicProcessor(LogicalSide.SinglePlayer, new SinglePlayerNetworkHandler(), saveName, settings);
        LogicProcessor.LoadMapping();
        LogicProcessor.Start();
        Player = new PlayerEntity();
        WorldRenderer = new WorldRenderer(Player);
        LogicProcessor.AddPlayer(Player);
        Player.SetWorld(LogicProcessor.GetWorld());
        Player.Pos = new Vector3D<double>(0, 0, 0);
    }

    public static void StartMultiplayer(string serverAddressAndPort)
    {
        var mapping = new RegistryMapping();
        //TODO receive mapping from server
        SynchronizedRegistries.LoadMapping(mapping);
        var ep = GetEndPoint(serverAddressAndPort);
        LogicProcessor = new LogicProcessor(LogicalSide.Client, new ClientNetworkHandler(ep), "remote");
        LogicProcessor.Start();
        Player = new PlayerEntity();
        WorldRenderer = new WorldRenderer(Player);
        LogicProcessor.AddPlayer(Player);
        Player.SetWorld(LogicProcessor.GetWorld());
        Player.Pos = new Vector3D<double>(0, 0, 0);
    }

    private static void OnWindowLoad()
    {
        WorldsDirectory = Path.GetFullPath("worlds");
        if (!Directory.Exists(WorldsDirectory))
        {
            Directory.CreateDirectory(WorldsDirectory);
        }

        ManagedENet.Startup();
        InputContext = Window.CreateInput();
        BfRendering.Init();
        BfDebug.Init();
        BfContent.Init();
        var a = typeof(BlockFactoryClient).Assembly;
        ResourceLoader = new AssemblyResourceLoader(a);
        BfClientContent.Init();
        MenuManager = new MenuManager();
        InputContext.Mice[0].MouseDown += MouseInputManager.MouseDown;
        InputContext.Mice[0].MouseUp += MouseInputManager.MouseUp;
        InputContext.Keyboards[0].KeyDown += KeyboardInputManager.KeyDown;
        InputContext.Keyboards[0].KeyChar += KeyboardInputManager.KeyChar;
    }

    public static void ExitWorld()
    {
        if (LogicProcessor != null)
        {
            WorldRenderer?.Dispose();
            WorldRenderer = null;
            if (LogicProcessor.LogicalSide != LogicalSide.Client)
            {
                LogicProcessor.SaveMapping();
            }

            LogicProcessor.Dispose();
            LogicProcessor = null;
            Player = null;
        }
    }

    private static void OnWindowClose()
    {
        ExitWorld();
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