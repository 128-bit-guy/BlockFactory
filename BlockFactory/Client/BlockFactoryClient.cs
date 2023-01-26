﻿using System.Drawing;
using BlockFactory.Render;
using BlockFactory.Render.Gui;
using BlockFactory.Render.Mesh;
using BlockFactory.Resource;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public class BlockFactoryClient
{
    private GLDebug _glDebug = null!;
    public GameWindow Window { get; private set; } = null!;
    public IResourceLoader ResourceLoader { get; private set; } = null!;
    public ClientContent ClientContent { get; private set; } = null!;
    public GuiRenderer GuiRenderer { get; private set; } = null!;
    public readonly VPMatrices VpMatrices = new();
    public readonly MatrixStack Matrices = new();

    public void Init()
    {
        var settings = new GameWindowSettings();
        settings.RenderFrequency = 0;
        settings.UpdateFrequency = 0;
        var nativeSettings = new NativeWindowSettings();
        nativeSettings.Profile = ContextProfile.Core;
        nativeSettings.Flags |= ContextFlags.Default | ContextFlags.Debug;
        nativeSettings.Title = "Block Factory";
        nativeSettings.NumberOfSamples = 4;
        nativeSettings.API = ContextAPI.OpenGL;
        nativeSettings.APIVersion = new Version(4, 6);
        Window = new GameWindow(settings, nativeSettings);
        GL.LoadBindings(new GLFWBindingsContext());
        Window.RenderFrame += UpdateAndRender;
        Window.Resize += _ => GL.Viewport(0, 0, Window.ClientSize.X, Window.ClientSize.Y);
        _glDebug = new GLDebug();
        _glDebug.Init();
        ResourceLoader = new AssemblyResourceLoader(typeof(BlockFactoryClient).Assembly);
        ClientContent = new ClientContent(this);
        GuiRenderer = new GuiRenderer(this);
    }

    public void UpdateAndRender(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GuiRenderer.UseGuiMatrices();
        Matrices.Push();
        Vector2 size = Window.ClientSize;
        var mid = size / 2;
        Matrices.Translate((mid.X, mid.Y, 50));
        GuiRenderer.DrawText("BlockFactory", 0);
        Matrices.Pop();
        Window.SwapBuffers();
    }

    public void Shutdown()
    {
        GuiRenderer.Dispose();
        GuiRenderer = null!;
        ClientContent.Dispose();
        ClientContent = null!;
        Window.Dispose();
    }

    public void Run()
    {
        Init();
        Window.Run();
        Shutdown();
    }
}