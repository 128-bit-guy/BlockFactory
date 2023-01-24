using BlockFactory.Serialization.Automatic;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public class BlockFactoryClient
{
    public GameWindow Window { get; private set; }

    public void Init()
    {
        var settings = new GameWindowSettings();
        settings.RenderFrequency = 0;
        settings.UpdateFrequency = 0;
        var nativeSettings = new NativeWindowSettings();
        nativeSettings.Profile = ContextProfile.Core;
        nativeSettings.Flags |= ContextFlags.Default;
        nativeSettings.Title = "Block Factory";
        nativeSettings.NumberOfSamples = 4;
        nativeSettings.API = ContextAPI.OpenGL;
        nativeSettings.APIVersion = new Version(4, 6);
        Window = new GameWindow(settings, nativeSettings);
        GL.LoadBindings(new GLFWBindingsContext());
        Window.RenderFrame += UpdateAndRender;
    }

    public void UpdateAndRender(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        Window.SwapBuffers();
    }

    public void Shutdown()
    {
        Window.Dispose();
    }

    public void Run()
    {
        Init();
        Window.Run();
        Shutdown();
    }
}