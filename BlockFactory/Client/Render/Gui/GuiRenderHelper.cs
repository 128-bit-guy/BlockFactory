using BlockFactory.Client.Render.Mesh_;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render.Gui;

public static class GuiRenderHelper
{
    public static readonly RenderMesh TextMesh = new(VertexBufferObjectUsage.StreamDraw);
    public static readonly MeshBuilder<GuiVertex> TextBuilder = new();

    public static unsafe void RenderText(TextRenderer renderer, ReadOnlySpan<char> s, int align)
    {
        renderer.Render(s, TextBuilder, align);
        TextBuilder.Upload(TextMesh);
        TextBuilder.Reset();
        renderer.BindTexture();
        Shaders.Text.Use();
        BfRendering.SetVpMatrices(Shaders.Text);
        Shaders.Text.SetModel(BfRendering.Matrices);
        Shaders.Text.Use();
        TextMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, TextMesh.IndexCount, DrawElementsType.UnsignedInt, null);
    }

    public static void RenderText(ReadOnlySpan<char> s, int align)
    {
        RenderText(BfClientContent.TextRenderer, s, align);
    }
}