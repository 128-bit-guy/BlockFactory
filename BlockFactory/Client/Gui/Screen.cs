using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Client.Render.Shader;
using BlockFactory.Item_;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class Screen : IDisposable
{
    public readonly BlockFactoryClient Client;
    public readonly StreamMesh<ColorVertex> ColorMesh;
    public readonly StreamMesh<GuiVertex> GuiMesh;
    public readonly List<Widget> Widgets;
    private Vector2i _lastSize;
    public Widget? ActiveWidget;

    public Screen(BlockFactoryClient client)
    {
        Client = client;
        ColorMesh = new StreamMesh<ColorVertex>(VertexFormats.Color);
        GuiMesh = new StreamMesh<GuiVertex>(VertexFormats.Gui);
        _lastSize = new Vector2i(-1, -1);
        Widgets = new List<Widget>();
    }

    public virtual void Dispose()
    {
        DestroyWidgets();
        GuiMesh.Dispose();
        ColorMesh.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual void OnShow()
    {
    }

    public virtual void OnHide()
    {
        DestroyWidgets();
        _lastSize = new Vector2i(-1, -1);
    }

    public virtual void UpdateAndRender()
    {
        DrawBackground();
        DrawWindow();
        Vector2i size = Client.GetDimensions();
        if (size != _lastSize)
        {
            _lastSize = size;
            DestroyWidgets();
            InitWidgets(size);
        }

        foreach (var widget in Widgets) widget.UpdateAndRender();
    }

    public virtual void DrawWindow()
    {
    }

    public virtual void InitWidgets(Vector2i size)
    {
    }

    protected void DestroyWidgets()
    {
        ActiveWidget = null;
        foreach (var widget in Widgets) widget.Dispose();
        Widgets.Clear();
    }

    public void DrawBackground()
    {
        var (width, height) = Client.GetDimensions();
        if (Client.GameInstance == null)
        {
            DrawTexturedRect(new Box2(0, 0, width, height), 0, 64, Textures.DirtTexture);
        }
        else
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            DrawColoredRect(new Box2(0, 0, width, height), 0, (0, 0, 0, 0.5f));
            GL.Disable(EnableCap.Blend);
        }
    }

    public void DrawTexturedRect(Box2 box, float zIndex, float textureScale, Texture2D texture)
    {
        GuiMesh.Builder.MatrixStack.Push();
        GuiMesh.Builder.BeginIndexSpace();
        GuiMesh.Builder.AddIndices(0, 2, 1, 0, 3, 2);
        var miX = box.Min.X;
        var maX = box.Max.X;
        var miY = box.Min.Y;
        var maY = box.Max.Y;
        var miTX = box.Min.X / textureScale;
        var maTX = box.Max.X / textureScale;
        var miTY = box.Min.Y / textureScale;
        var maTY = box.Max.Y / textureScale;
        GuiMesh.Builder.AddVertex((miX, miY, zIndex, 1, 1, 1, miTX, miTY));
        GuiMesh.Builder.AddVertex((maX, miY, zIndex, 1, 1, 1, maTX, miTY));
        GuiMesh.Builder.AddVertex((maX, maY, zIndex, 1, 1, 1, maTX, maTY));
        GuiMesh.Builder.AddVertex((miX, maY, zIndex, 1, 1, 1, miTX, maTY));
        GuiMesh.Builder.EndIndexSpace();
        GuiMesh.Builder.MatrixStack.Pop();
        Shaders.Gui.Use();
        Client.VpMatrices.Set(Shaders.Gui);
        Shaders.Gui.SetModel(Client.Matrices);
        texture.BindTexture();
        GuiMesh.Flush();
    }

    public void DrawColoredRect(Box2 box, float zIndex, Vector4 color)
    {
        ColorMesh.Builder.MatrixStack.Push();
        ColorMesh.Builder.BeginIndexSpace();
        ColorMesh.Builder.AddIndices(0, 2, 1, 0, 3, 2);
        var miX = box.Min.X;
        var maX = box.Max.X;
        var miY = box.Min.Y;
        var maY = box.Max.Y;
        ColorMesh.Builder.AddVertex((miX, miY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMesh.Builder.AddVertex((maX, miY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMesh.Builder.AddVertex((maX, maY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMesh.Builder.AddVertex((miX, maY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMesh.Builder.EndIndexSpace();
        ColorMesh.Builder.MatrixStack.Pop();
        Shaders.Color.Use();
        Client.VpMatrices.Set(Shaders.Color);
        Shaders.Color.SetModel(Client.Matrices);
        Shaders.Color.SetColor(Vector4.One);
        ColorMesh.Flush();
    }

    public void DrawText(ReadOnlySpan<char> text, int align)
    {
        GuiMesh.Builder.MatrixStack.Push();
        Textures.TextRenderer.Render(text, GuiMesh.Builder, align);
        GuiMesh.Builder.MatrixStack.Pop();
        Shaders.Text.Use();
        Client.VpMatrices.Set(Shaders.Text);
        Shaders.Text.SetModel(Client.Matrices);
        Textures.TextRenderer.BindTexture();
        GuiMesh.Flush();
    }

    public void DrawStack(ItemStack stack)
    {
        Client.Matrices.Push();
        Client.Matrices.Scale(new Vector3(-32.0f, -32.0f, 1.0f));
        Client.Matrices.RotateX(-0.62f);
        Client.Matrices.RotateY(MathF.PI / 4);
        Client.Matrices.Translate((-0.5f, -0.5f, -0.5f));
        Client.ItemRenderer!.RenderItemStack(stack);
        Client.Matrices.Pop();
        if (!stack.IsEmpty())
        {
            Client.Matrices.Push();
            Client.Matrices.Translate(new Vector3(0f, 0f, 2f));
            Client.Matrices.Scale(0.5f);
            DrawText(stack.Count.ToString(), 0);
            Client.Matrices.Pop();
        }
    }
}