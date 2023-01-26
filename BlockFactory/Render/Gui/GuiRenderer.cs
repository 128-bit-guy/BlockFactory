using BlockFactory.Client;
using BlockFactory.Render.Mesh;
using BlockFactory.Render.Mesh.Vertex;
using BlockFactory.Render.Shader;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Render.Gui;

[ExclusiveTo(Side.Client)]
public class GuiRenderer : IDisposable
{
    public readonly BlockFactoryClient Client;
    public readonly MeshBuilder<GuiVertex> GuiMeshBuilder;
    public readonly RenderMesh<GuiVertex> GuiMesh;
    public readonly MeshBuilder<ColorVertex> ColorMeshBuilder;
    public readonly RenderMesh<ColorVertex> ColorMesh;
    private readonly TextRenderer _textRenderer;
    private readonly Shaders _shaders;

    public GuiRenderer(BlockFactoryClient client)
    {
        Client = client;
        VertexFormats vertexFormats = Client.ClientContent.VertexFormats;
        GuiMeshBuilder = new MeshBuilder<GuiVertex>(vertexFormats.Gui);
        GuiMesh = new RenderMesh<GuiVertex>(vertexFormats.Gui);
        ColorMeshBuilder = new MeshBuilder<ColorVertex>(vertexFormats.Color);
        ColorMesh = new RenderMesh<ColorVertex>(vertexFormats.Color);
        _textRenderer = client.ClientContent.TextRenderer;
        _shaders = client.ClientContent.Shaders;
    }

    public void UseGuiMatrices()
    {
        Client.VpMatrices.View = Matrix4.Identity;
        var (width, height) = Client.Window.ClientSize;
        Client.VpMatrices.Projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -100, 100);
    }

    public void DrawTexturedRect(Box2 box, float zIndex, float textureScale, Texture2D texture)
    {
        GuiMeshBuilder.MatrixStack.Push();
        GuiMeshBuilder.BeginIndexSpace();
        GuiMeshBuilder.AddIndices(0, 2, 1, 0, 3, 2);
        float miX = box.Min.X;
        float maX = box.Max.X;
        float miY = box.Min.Y;
        float maY = box.Max.Y;
        float miTX = box.Min.X / textureScale;
        float maTX = box.Max.X / textureScale;
        float miTY = box.Min.Y / textureScale;
        float maTY = box.Max.Y / textureScale;
        GuiMeshBuilder.AddVertex((miX, miY, zIndex, 1, 1, 1, miTX, miTY));
        GuiMeshBuilder.AddVertex((maX, miY, zIndex, 1, 1, 1, maTX, miTY));
        GuiMeshBuilder.AddVertex((maX, maY, zIndex, 1, 1, 1, maTX, maTY));
        GuiMeshBuilder.AddVertex((miX, maY, zIndex, 1, 1, 1, miTX, maTY));
        GuiMeshBuilder.EndIndexSpace();
        GuiMeshBuilder.MatrixStack.Pop();
        GuiMeshBuilder.Upload(GuiMesh);
        GuiMeshBuilder.Reset();
        GuiMesh.Bind();
        _shaders.Gui.Use();
        Client.VpMatrices.Set(_shaders.Gui);
        _shaders.Gui.SetModel(Client.Matrices);
        texture.BindTexture();
        GL.DrawElements(PrimitiveType.Triangles, GuiMesh.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void DrawColoredRect(Box2 box, float zIndex, Vector4 color) {
        ColorMeshBuilder.MatrixStack.Push();
        ColorMeshBuilder.BeginIndexSpace();
        ColorMeshBuilder.AddIndices(0, 2, 1, 0, 3, 2);
        float miX = box.Min.X;
        float maX = box.Max.X;
        float miY = box.Min.Y;
        float maY = box.Max.Y;
        ColorMeshBuilder.AddVertex((miX, miY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMeshBuilder.AddVertex((maX, miY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMeshBuilder.AddVertex((maX, maY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMeshBuilder.AddVertex((miX, maY, zIndex, color.X, color.Y, color.Z, color.W));
        ColorMeshBuilder.EndIndexSpace();
        ColorMeshBuilder.MatrixStack.Pop();
        ColorMeshBuilder.Upload(ColorMesh);
        ColorMeshBuilder.Reset();
        ColorMesh.Bind();
        _shaders.Color.Use();
        Client.VpMatrices.Set(_shaders.Color);
        _shaders.Color.SetModel(Client.Matrices);
        _shaders.Color.SetColor(Vector4.One);
        GL.DrawElements(PrimitiveType.Triangles, ColorMesh.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void DrawText(ReadOnlySpan<char> text, int align) {
        GuiMeshBuilder.MatrixStack.Push();
        _textRenderer.Render(text, GuiMeshBuilder, align);
        GuiMeshBuilder.MatrixStack.Pop();
        GuiMeshBuilder.Upload(GuiMesh);
        GuiMeshBuilder.Reset();
        _shaders.Text.Use();
        Client.VpMatrices.Set(_shaders.Text);
        _shaders.Text.SetModel(Client.Matrices);
        _textRenderer.BindTexture();
        GuiMesh.Bind();
        GL.DrawElements(PrimitiveType.Triangles, GuiMesh.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose()
    {
        GuiMesh.Dispose();
        ColorMesh.Dispose();
    }
}