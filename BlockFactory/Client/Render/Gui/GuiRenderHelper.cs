using System.Drawing;
using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Entity_;
using BlockFactory.Math_;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Texture = BlockFactory.Client.Render.Texture_.Texture;

namespace BlockFactory.Client.Render.Gui;

[ExclusiveTo(Side.Client)]
public static class GuiRenderHelper
{
    private static readonly uint[] QuadIndices = { 0, 2, 1, 0, 3, 2 };
    public static readonly RenderMesh GuiMesh = new(VertexBufferObjectUsage.StreamDraw);
    public static readonly MeshBuilder<GuiVertex> GuiBuilder = new();
    public static MeshBuilder<GuiVertex> TexturedGuiBuilder;
    public static TextureAtlasUvTransformer Transformer;

    public static void Init()
    {
        Transformer = new TextureAtlasUvTransformer(Textures.Gui);
        TexturedGuiBuilder = new MeshBuilder<GuiVertex>(Transformer);
    }

    public static unsafe void RenderText(TextRenderer renderer, ReadOnlySpan<char> s, int align, Color color)
    {
        GuiBuilder.Color = color;
        renderer.Render(s, GuiBuilder, align);
        GuiBuilder.Upload(GuiMesh);
        GuiBuilder.Reset();
        renderer.BindTexture();
        Shaders.Text.Use();
        BfRendering.SetVpMatrices(Shaders.Text);
        Shaders.Text.SetModel(BfRendering.Matrices);
        Shaders.Text.Use();
        GuiMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, GuiMesh.IndexCount, DrawElementsType.UnsignedInt, null);
    }

    public static void RenderText(ReadOnlySpan<char> s, int align, Color color)
    {
        RenderText(BfClientContent.TextRenderer, s, align, color);
    }

    public static void RenderText(ReadOnlySpan<char> s, int align)
    {
        RenderText(BfClientContent.TextRenderer, s, align, Color.White);
    }

    private static void BuildTexturedQuad(Box2D<float> box, Box2D<float> texBox)
    {
        TexturedGuiBuilder.Matrices.Push();
        TexturedGuiBuilder.NewPolygon();
        TexturedGuiBuilder.Vertex(new GuiVertex(box.Min.X, box.Min.Y, 0f, 1f, 1f, 1f, 1f, texBox.Min.X, texBox.Min.Y));
        TexturedGuiBuilder.Vertex(new GuiVertex(box.Max.X, box.Min.Y, 0f, 1f, 1f, 1f, 1f, texBox.Max.X, texBox.Min.Y));
        TexturedGuiBuilder.Vertex(new GuiVertex(box.Max.X, box.Max.Y, 0f, 1f, 1f, 1f, 1f, texBox.Max.X, texBox.Max.Y));
        TexturedGuiBuilder.Vertex(new GuiVertex(box.Min.X, box.Max.Y, 0f, 1f, 1f, 1f, 1f, texBox.Min.X, texBox.Max.Y));
        TexturedGuiBuilder.Indices(QuadIndices);
        TexturedGuiBuilder.Matrices.Pop();
    }

    private static unsafe void RenderBufferContent()
    {
        TexturedGuiBuilder.Upload(GuiMesh);
        TexturedGuiBuilder.Reset();
        Textures.Gui.Bind();
        Shaders.Gui.Use();
        BfRendering.SetVpMatrices(Shaders.Gui);
        Shaders.Gui.SetModel(BfRendering.Matrices);
        Shaders.Gui.Use();
        GuiMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, GuiMesh.IndexCount, DrawElementsType.UnsignedInt, null);
    }

    public static void RenderTexturedQuad(int texture, Box2D<float> box, Box2D<float> texBox)
    {
        Transformer.Sprite = texture;
        BuildTexturedQuad(box, texBox);
        RenderBufferContent();
    }

    public static void RenderTexturedQuad(int texture, Box2D<float> box)
    {
        RenderTexturedQuad(texture, box, new Box2D<float>(0, 0, 1, 1));
    }

    public static void RenderQuadWithBorder(int texture, Box2D<float> box, float padding, float texPadding)
    {
        Transformer.Sprite = texture;
        var intBox =
            new Box2D<float>(box.Min + new Vector2D<float>(padding), box.Max - new Vector2D<float>(padding));
        var repeatsX = Math.Max((int)MathF.Ceiling(intBox.Size.X / padding * (2 * texPadding)), 1);
        var repeatsY = Math.Max((int)MathF.Ceiling(intBox.Size.Y / padding * (2 * texPadding)), 1);
        for (var i = 0; i < repeatsX + 2; ++i)
        for (var j = 0; j < repeatsY + 2; ++j)
        {
            var renderBox = new Box2D<float>();
            var texBox = new Box2D<float>();
            if (i == 0)
            {
                renderBox.Min.X = box.Min.X;
                renderBox.Max.X = intBox.Min.X;
                texBox.Min.X = 0;
                texBox.Max.X = texPadding;
            }
            else if (i == repeatsX + 1)
            {
                renderBox.Min.X = intBox.Max.X;
                renderBox.Max.X = box.Max.X;
                texBox.Min.X = 1 - texPadding;
                texBox.Max.X = 1;
            }
            else
            {
                var repI = i - 1;
                var minRepF = (float)repI / repeatsX - 1e-5f;
                var maxRepF = (float)(repI + 1) / repeatsX + 1e-5f;
                renderBox.Min.X = BfMathUtils.Lerp(minRepF, intBox.Min.X, intBox.Max.X);
                renderBox.Max.X = BfMathUtils.Lerp(maxRepF, intBox.Min.X, intBox.Max.X);
                texBox.Min.X = texPadding;
                texBox.Max.X = 1 - texPadding;
            }

            if (j == 0)
            {
                renderBox.Min.Y = box.Min.Y;
                renderBox.Max.Y = intBox.Min.Y;
                texBox.Min.Y = 1;
                texBox.Max.Y = 1 - texPadding;
            }
            else if (j == repeatsY + 1)
            {
                renderBox.Min.Y = intBox.Max.Y;
                renderBox.Max.Y = box.Max.Y;
                texBox.Min.Y = texPadding;
                texBox.Max.Y = 0;
            }
            else
            {
                var repI = j - 1;
                var minRepF = (float)repI / repeatsY - 1e-5f;
                var maxRepF = (float)(repI + 1) / repeatsY + 1e-5f;
                renderBox.Min.Y = BfMathUtils.Lerp(minRepF, intBox.Min.Y, intBox.Max.Y);
                renderBox.Max.Y = BfMathUtils.Lerp(maxRepF, intBox.Min.Y, intBox.Max.Y);
                texBox.Min.Y = 1 - texPadding;
                texBox.Max.Y = texPadding;
            }

            BuildTexturedQuad(renderBox, texBox);
        }

        RenderBufferContent();
    }
}