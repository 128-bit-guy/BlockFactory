using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Shader;
using BlockFactory;
using BlockFactory.Client.Entity_;
using BlockFactory.Client.Game;
using BlockFactory.Client.Gui;
using BlockFactory.Client.Gui.InGame;
using BlockFactory.Client.Init;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.World_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Gui;
using BlockFactory.Init;
using BlockFactory.Network;
using BlockFactory.Util.Math_;
using ClearBufferMask = OpenTK.Graphics.OpenGL4.ClearBufferMask;
using CullFaceMode = OpenTK.Graphics.OpenGL4.CullFaceMode;
using DepthFunction = OpenTK.Graphics.OpenGL4.DepthFunction;
using EnableCap = OpenTK.Graphics.OpenGL4.EnableCap;
using GL = OpenTK.Graphics.OpenGL4.GL;

namespace BlockFactory.Client;

public class BlockFactoryClient
{
    public unsafe Window* Window { get; private set; }
    public static readonly BlockFactoryClient Instance = new();
    public ClientPlayerEntity? Player = null;
    public VPMatrices VpMatrices = null!;
    private DateTime _lastTime;
    public TimeSpan DeltaTime { get; private set; }
    public NetworkConnection? ServerConnection;
    public GameInstance? GameInstance;
    private Vector2d _cursorPos;
    public Vector2d CursorPos { get { return _cursorPos; } }
    public Vector2d CursorPosDelta { get; private set; }
    public WorldRenderer? WorldRenderer;
    public HudRenderer? HudRenderer;
    public ItemRenderer? ItemRenderer;
    private readonly Stack<Screen> _screenStack = new();
    public readonly MatrixStack Matrices;
    public bool CursorVisible;
    public delegate void MouseButtonHandler(MouseButton button, InputAction action, KeyModifiers modifiers);
    public event MouseButtonHandler OnMouseButton;
    public delegate void CharInputHandler(string chars);
    public event CharInputHandler OnCharInput;
    public delegate void KeyInputHandler(Keys key, int scancode, InputAction action, KeyModifiers mods);
    public event KeyInputHandler OnKeyInput;
    private GLFWCallbacks.FramebufferSizeCallback _fbscb = null!;
    private GLFWCallbacks.MouseButtonCallback _mbcb = null!;
    private GLFWCallbacks.CharCallback _ccb = null!;
    private GLFWCallbacks.KeyCallback _kcb = null!;
    private GLFWCallbacks.ScrollCallback _scb = null!;
    private bool _shouldRun;

    private BlockFactoryClient()
    {
        Matrices = new MatrixStack();
        OnMouseButton = (_, _, _) => { };
        OnCharInput = (_) => { };
        OnKeyInput = OnKeyInput0;
    }

    private void OnKeyInput0(Keys key, int scancode, InputAction action, KeyModifiers modifiers)
    {
        if (action == InputAction.Press)
        {
            if (GameInstance != null)
            {
                if (key == Keys.Escape)
                {
                    if (HasScreen())
                    {
                        if (_screenStack.Peek() is InGameMenuScreen || _screenStack.Peek() is ChatScreen)
                        {
                            Player!.HandlePlayerAction(PlayerActionType.ChangeMenu, 0);
                        }
                        else
                        {
                            PopScreen();
                        }
                    }
                    else
                    {
                        PushScreen(new PauseMenuScreen(this));
                    }
                } else if (key == Keys.E)
                {
                    if (!HasScreen() || _screenStack.Peek() is InGameMenuScreen)
                    {
                        Player!.HandlePlayerAction(PlayerActionType.ChangeMenu, 1);
                    }
                } else if (key == Keys.T)
                {
                    if (!HasScreen())
                    {
                        PushScreen(new ChatScreen(this));
                    }
                }
            }
        }

        if (key is >= Keys.D1 and <= Keys.D9)
        {
            if (Player != null)
            {
                Player.HandlePlayerAction(PlayerActionType.SetHotbarPos, key - Keys.D1);
            }
        }
    }

    private unsafe void InitWindow()
    {
        GLFW.Init();
        GLFW.DefaultWindowHints();
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
        Window = GLFW.CreateWindow(200, 200, "Voxel Builder", null, null);
        GLFW.ShowWindow(Window);
        GLFW.MakeContextCurrent(Window);
        GL.LoadBindings(new GLFWBindingsContext());
        GLFW.SetFramebufferSizeCallback(Window, _fbscb = (_, width, height) => GL.Viewport(0, 0, width, height));
        GLFW.SetMouseButtonCallback(Window, _mbcb = (w, button, action, modifiers) => OnMouseButton(button, action, modifiers));
        GLFW.SetCharCallback(Window, _ccb = (w, ch) => OnCharInput(char.ConvertFromUtf32(unchecked((int)ch))));
        GLFW.SetKeyCallback(Window, _kcb = (w, key, scancode, action, mods) => OnKeyInput(key, scancode, action, mods));
        GLFW.SetScrollCallback(Window, _scb = (w, dx, dy) => OnScroll(dx, dy));
        _shouldRun = true;
    }

    private void OnScroll(double dX, double dY)
    {
        if (Player != null)
        {
            if (dY > 0)
            {
                Player.HandlePlayerAction(PlayerActionType.AddHotbarPos, 1);
            }
            else
            {
                Player.HandlePlayerAction(PlayerActionType.AddHotbarPos, -1);
            }
        }
    }

    public void InitMultiplayerGameInstance(string serverAddressAndPort)
    {
        Socket socket;
        Player = new ClientPlayerEntity
        {
            Pos = new EntityPos((0, 0, 10))
        };
        GameInstance = new GameInstance(GameKind.MultiplayerFrontend, Thread.CurrentThread,
            unchecked((int)DateTime.UtcNow.Ticks), "-")
        {
            NetworkHandler = new MultiplayerFrontendNetworkHandler(this),
            SideHandler = new ClientSideHandler(this)

        };
        if (serverAddressAndPort.Length == 0)
        {
            serverAddressAndPort = Dns.GetHostName();
        }
        if (!serverAddressAndPort.Contains(':'))
        {
            serverAddressAndPort += ":" + Constants.DefaultPort;
        }
        var a = serverAddressAndPort.Split(':');
        var address = a[0];
        var port = int.Parse(a[1]);
        var entry = Dns.GetHostEntry(address);
        var ipAddress = entry.AddressList[0];
        var server = new IPEndPoint(ipAddress, port);
        socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(server);
        ServerConnection = new NetworkConnection(socket)
        {
            GameInstance = GameInstance
        };
        ServerConnection.Start();
        GameInstance.Init();
        GameInstance.World.AddPlayer(Player);
        WorldRenderer = new WorldRenderer(GameInstance.World, Player);
        HudRenderer = new HudRenderer(this, WorldRenderer);
        ItemRenderer = new ItemRenderer(this, WorldRenderer);
        Player.OnMenuChange += OnInGameMenuOpen;
    }

    public void InitSingleplayerGameInstance() {
        Player = new ClientPlayerEntity
        {
            Pos = new EntityPos((0, 0, 10))
        };
        GameInstance = new GameInstance(GameKind.Singleplayer, Thread.CurrentThread,
            unchecked((int)DateTime.UtcNow.Ticks), Path.GetFullPath("world"))
        {
            NetworkHandler = new SingleplayerNetworkHandler(),
            SideHandler = new ClientSideHandler(this)
        };
        GameInstance.Init();
        GameInstance.World.AddPlayer(Player);
        WorldRenderer = new WorldRenderer(GameInstance.World, Player);
        HudRenderer = new HudRenderer(this, WorldRenderer);
        ItemRenderer = new ItemRenderer(this, WorldRenderer);
        Player.OnMenuChange += OnInGameMenuOpen;
    }

    private unsafe void Init()
    {
        CursorVisible = true;
        InitWindow();
        CommonInit.Init();
        ClientInit.Init();
        PushScreen(new MainMenuScreen(this));
        //InitGameInstance();
        VpMatrices = new VPMatrices();
        GL.ClearColor(Color.SkyBlue);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);
    }

    private void UseWorldMatrices()
    {
        VpMatrices.View = Matrix4.LookAt(Player!.GetInterpolatedPos().PosInChunk,
            Player!.GetInterpolatedPos().PosInChunk + Player!.GetForward(), Player.GetUp());
        VpMatrices.Projection = CreatePerspective();
    }

    public Matrix4 CreatePerspective()
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, GetAspectRatio(), 0.1f, 1000f);
    }

    private void UseGuiMatrices()
    {
        VpMatrices.View = Matrix4.Identity;
        var (width, height) = GetDimensions();
        VpMatrices.Projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -100, 100);
    }

    private void Use3DHudMatrices()
    {
        VpMatrices.View = Matrix4.Identity;
        VpMatrices.Projection = CreatePerspective();
    }

    public unsafe (int, int) GetDimensions()
    {
        GLFW.GetFramebufferSize(Window, out var width, out var height);
        return (width, height);
    }

    public unsafe bool IsKeyPressed(Keys key)
    {
        return GLFW.GetKey(Window, key) == InputAction.Press;
    }

    public unsafe bool IsMouseButtonPressed(MouseButton button)
    {
        return GLFW.GetMouseButton(Window, button) == InputAction.Press;
    }

    public float GetAspectRatio()
    {
        var (width, height) = GetDimensions();
        return (float)width / height;
    }

    private void UpdateTime()
    {
        DateTime now = DateTime.UtcNow;
        DeltaTime = now - _lastTime;
        _lastTime = now;
    }

    private unsafe void ShowCursor()
    {
        if (!CursorVisible)
        {
            CursorVisible = true;
            GLFW.SetInputMode(Window, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
            var (width, height) = GetDimensions();
            GLFW.SetCursorPos(Window, width * 0.5d, height * 0.5d);
        }
    }

    private unsafe void HideCursor()
    {
        if (CursorVisible)
        {
            CursorVisible = false;
            GLFW.SetInputMode(Window, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
        }
    }

    private void UpdateAndRender()
    {
        Matrices.Push();
        if (GameInstance != null && _screenStack.Count == 0)
        {
            HideCursor();
        }
        else
        {
            ShowCursor();
        }
        UpdateTime();
        UpdateCursor();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        if (GameInstance != null)
        {
            if (!HasScreen())
            {
                Player!.HeadRotation -= (Vector2)CursorPosDelta * 0.001f;
            }
            GameInstance!.Update();
            UseWorldMatrices();
            WorldRenderer!.UpdateAndRender();
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            Use3DHudMatrices();
            HudRenderer!.Render3DHud();
            if (GameInstance.Kind.IsNetworked() && ServerConnection!.Errored)
            {
                while (HasScreen())
                {
                    PopScreen();
                }
                PushScreen(new MainMenuScreen(this));
                PushScreen(new ForcedDisconnectionScreen(this, ServerConnection.LastError!));
                CleanupGameInstance();
            }
        }
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        UseGuiMatrices();
        if (HasScreen())
        {
            _screenStack.Peek().UpdateAndRender();
        }
        Matrices.Pop();
    }

    public void PushScreen(Screen screen)
    {
        if (_screenStack.Count > 0)
        {
            _screenStack.Peek().OnHide();
        }
        _screenStack.Push(screen);
        screen.OnShow();
    }

    public Vector3 GetDirectionFromPosFor3DHud(Vector2 pos)
    {
        pos.Y = -pos.Y;
        return (Vector3.Unproject(new Vector3(pos), -1, -1, 2, 2, -1, 1,
            (CreatePerspective()).Inverted())).Normalized();
    }

    public void PopScreen()
    {
        _screenStack.Peek().OnHide();
        _screenStack.Pop().Dispose();
        if (_screenStack.Count > 0)
        {
            _screenStack.Peek().OnShow();
        }
    }

    public bool HasScreen()
    {
        return _screenStack.Count > 0;
    }

    private unsafe void UpdateCursor()
    {
        Vector2d lastCursorPos = CursorPos;
        GLFW.GetCursorPos(Window, out _cursorPos.X, out _cursorPos.Y);
        CursorPosDelta = CursorPos - lastCursorPos;
    }

    public void CleanupGameInstance()
    {
        if (ServerConnection != null)
        {
            ServerConnection!.Stop();
            ServerConnection.Dispose();
            ServerConnection = null;
        }

        if (Player != null)
        {
            Player.OnMenuChange -= OnInGameMenuOpen;
        }
        Player = null;
        if (GameInstance != null)
        {
            GameInstance.Dispose();
            GameInstance = null;
        }

        if (HudRenderer != null)
        {
            HudRenderer!.Dispose();
            HudRenderer = null;
        }

        if (ItemRenderer != null)
        {
            ItemRenderer!.Dispose();
            ItemRenderer = null;
        }
        if (WorldRenderer != null)
        {
            WorldRenderer!.Dispose();
            WorldRenderer = null;
        }
    }

    private void OnInGameMenuOpen(InGameMenu? previous, InGameMenu? menu)
    {
        while (HasScreen())
        {
            PopScreen();
        }

        if (menu != null)
        {
            PushScreen(InGameMenuScreens.ScreenCreators[menu.Type](menu, this));
        }
    }

    public void Stop()
    {
        _shouldRun = false;
    }

    internal unsafe void Run()
    {
        Init();
        while (!GLFW.WindowShouldClose(Window) && _shouldRun)
        {
            GLFW.PollEvents();
            UpdateAndRender();
            GLFW.SwapBuffers(Window);
        }
        if (GameInstance != null)
        {
            CleanupGameInstance();
        }
        GLFW.Terminate();
    }
}