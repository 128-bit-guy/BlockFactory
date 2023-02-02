using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using StbTrueTypeSharp;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class TextRenderer
{
    private static readonly int[] QuadIndices = { 0, 2, 1, 0, 3, 2 };
    private readonly StbTrueType.stbtt_bakedchar[] _chars;
    private readonly int _tex;
    private readonly int _width, _height;
    public readonly float Ascent, Descent, LineGap;

    public TextRenderer(Stream s, float pixelHeight)
    {
        var bitmapWidth = 512;
        var bitmapHeight = 512;
        byte[] data;
        using (var ms = new MemoryStream())
        {
            s.CopyTo(ms);
            data = ms.ToArray();
        }

        float ascent, descent, lineGap;
        unsafe
        {
            fixed (byte* dataptr = data)
            {
                StbTrueType.stbtt_GetScaledFontVMetrics(dataptr, 0, pixelHeight, &ascent, &descent, &lineGap);
            }
        }

        Ascent = ascent;
        Descent = descent;
        LineGap = lineGap;
        while (true)
        {
            var bitmap = new byte[bitmapWidth * bitmapHeight];
            _chars = new StbTrueType.stbtt_bakedchar[96];
            if (BakeFontBitmap(data, 0, pixelHeight, bitmap, bitmapWidth, bitmapHeight, 32, 96, _chars) <= 0)
            {
                bitmapWidth *= 2;
                bitmapHeight *= 2;
                continue;
            }

            _tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _tex);
            GLPreconditions.CheckGLError();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, bitmapWidth, bitmapHeight, 0,
                PixelFormat.Red, PixelType.UnsignedByte, bitmap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GLPreconditions.CheckGLError();
            _width = bitmapWidth;
            _height = bitmapHeight;
            break;
        }
    }

    private static unsafe int BakeFontBitmap(byte[] ttf, int offset, float pixel_height, byte[] pixels, int pw,
        int ph,
        int first_char, int num_chars, StbTrueType.stbtt_bakedchar[] chardata)
    {
        fixed (byte* ttfPtr = ttf)
        {
            fixed (byte* pixelsPtr = pixels)
            {
                fixed (StbTrueType.stbtt_bakedchar* chardataPtr = chardata)
                {
                    var result = StbTrueType.stbtt_BakeFontBitmap(ttfPtr, offset, pixel_height, pixelsPtr, pw, ph,
                        first_char,
                        num_chars,
                        chardataPtr);

                    return result;
                }
            }
        }
    }

    public void Render(ReadOnlySpan<char> s, MeshBuilder<GuiVertex> meshBuilder)
    {
        float x = 0;
        float y = 0;
        foreach (var c in s)
        {
            var c1 = Convert.ToInt32(c);
            if (c1 >= 32 && c <= 128)
            {
                StbTrueType.stbtt_aligned_quad quad;
                unsafe
                {
                    fixed (StbTrueType.stbtt_bakedchar* chars = _chars)
                    {
                        StbTrueType.stbtt_GetBakedQuad(chars, _width, _height, c1 - 32, &x, &y, &quad, 1);
                    }
                }

                meshBuilder.BeginIndexSpace();
                meshBuilder.AddVertex((quad.x0, quad.y0, 0f, 1f, 1f, 1f, quad.s0, quad.t0));
                meshBuilder.AddVertex((quad.x1, quad.y0, 0f, 1f, 1f, 1f, quad.s1, quad.t0));
                meshBuilder.AddVertex((quad.x1, quad.y1, 0f, 1f, 1f, 1f, quad.s1, quad.t1));
                meshBuilder.AddVertex((quad.x0, quad.y1, 0f, 1f, 1f, 1f, quad.s0, quad.t1));
                meshBuilder.AddIndices(QuadIndices);
                meshBuilder.EndIndexSpace();
            }
        }
    }

    public float GetStringHeight(ReadOnlySpan<char> s)
    {
        var result = Ascent - Descent;
        foreach (var c in s)
            if (c == '\n')
                result += Ascent - Descent + LineGap;
        return result;
    }

    public float GetStringWidth(ReadOnlySpan<char> s)
    {
        float min = 0, max = 0;
        float x = 0, y = 0;
        foreach (var c in s)
            if (c == '\n')
            {
                x = 0;
            }
            else
            {
                var c1 = Convert.ToInt32(c);
                if (c1 >= 32 && c <= 128)
                {
                    StbTrueType.stbtt_aligned_quad quad;
                    unsafe
                    {
                        fixed (StbTrueType.stbtt_bakedchar* chars = _chars)
                        {
                            StbTrueType.stbtt_GetBakedQuad(chars, _width, _height, c1 - 32, &x, &y, &quad, 1);
                        }
                    }

                    min = MathF.Min(min, quad.x0);
                    max = MathF.Max(max, quad.x1);
                }
            }

        return max - min;
    }

    public void Render(ReadOnlySpan<char> s, MeshBuilder<GuiVertex> meshBuilder, int align)
    {
        var pos = 0;
        var currentY = Ascent;
        while (pos <= s.Length)
        {
            var nextX = s.Slice(pos).IndexOf('\n');
            if (nextX == -1)
                nextX = s.Length;
            else
                nextX += pos;
            var subSpan = s.Slice(pos, nextX - pos);
            var width = GetStringWidth(subSpan);
            meshBuilder.MatrixStack.Push();
            meshBuilder.MatrixStack.Translate((-width / 2 * (align + 1), currentY, 0f));
            Render(subSpan, meshBuilder);
            meshBuilder.MatrixStack.Pop();
            currentY += Ascent - Descent + LineGap;
            pos = nextX + 1;
        }
    }

    public void BindTexture()
    {
        GL.BindTexture(TextureTarget.Texture2D, _tex);
    }
}