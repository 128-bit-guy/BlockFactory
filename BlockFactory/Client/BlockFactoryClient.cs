using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Resource;
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
    private static uint _shaderProgram;
    private static uint _vertexArray;
    private static uint _vertexBuffer;
    private static uint _indexBuffer;

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
        BfRendering.Gl.UseProgram(_shaderProgram);
        BfRendering.Gl.BindVertexArray(_vertexArray);
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, null);
        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfDebug.UpdateAndRender(deltaTime);
    }

    private static void OnWindowLoad()
    {
        var a = typeof(BlockFactoryClient).Assembly;
        ResourceLoader = new AssemblyResourceLoader(a);
        var vs = BfRendering.Gl.CreateShader(ShaderType.VertexShader);
        BfRendering.Gl.ShaderSource(vs, ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Vertex.glsl"));
        BfRendering.Gl.CompileShader(vs);
        var fs = BfRendering.Gl.CreateShader(ShaderType.FragmentShader);
        BfRendering.Gl.ShaderSource(fs, ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Fragment.glsl"));
        BfRendering.Gl.CompileShader(fs);
        _shaderProgram = BfRendering.Gl.CreateProgram();
        BfRendering.Gl.AttachShader(_shaderProgram, vs);
        BfRendering.Gl.AttachShader(_shaderProgram, fs);
        BfRendering.Gl.LinkProgram(_shaderProgram);
        BfRendering.Gl.DeleteShader(vs);
        BfRendering.Gl.DeleteShader(fs);
        Vector3D<float>[] verts = { new(0, 1, 0), new(1, 0, 0), new(-1, 0, 0) };
        uint[] inds = { 0, 1, 2 };
        _vertexBuffer = BfRendering.Gl.CreateBuffer();
        BfRendering.Gl.NamedBufferData<Vector3D<float>>(_vertexBuffer, verts, VertexBufferObjectUsage.StaticDraw);
        _indexBuffer = BfRendering.Gl.CreateBuffer();
        BfRendering.Gl.NamedBufferData<uint>(_indexBuffer, inds, VertexBufferObjectUsage.StaticDraw);
        _vertexArray = BfRendering.Gl.CreateVertexArray();
        VertexInfo<BlockVertex>.AttachVbo(_vertexArray, _vertexBuffer, 0);
        BfRendering.Gl.VertexArrayElementBuffer(_vertexArray, _indexBuffer);
    }

    private static void OnWindowClose()
    {
        BfRendering.Gl.DeleteProgram(_shaderProgram);
        BfRendering.Gl.DeleteVertexArray(_vertexArray);
        BfRendering.Gl.DeleteBuffer(_vertexBuffer);
        BfRendering.Gl.DeleteBuffer(_indexBuffer);
        BfDebug.OnWindowClose();
    }

    private static void AddEvents()
    {
        Window.Load += BfRendering.OnWindowLoad;
        Window.Load += BfDebug.OnWindowLoad;
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