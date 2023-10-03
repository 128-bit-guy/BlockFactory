using System.Drawing;
using BlockFactory.Base;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class Image
{
    public readonly byte[] Data;
    public readonly int Width, Height;

    public Image(ImageResult result)
    {
        Width = result.Width;
        Height = result.Height;
        Data = (byte[])result.Data.Clone();
    }

    public Image(int width, int height, Color color)
    {
        Width = width;
        Height = height;
        Data = new byte[4 * width * height];
        for (var x = 0; x < Width; ++x)
        for (var y = 0; y < Height; ++y)
            this[x, y] = color;
    }

    public Image(int width, int height) : this(width, height, Color.Black)
    {
    }

    public Color this[int x, int y]
    {
        get
        {
            var idx = x * 4 + y * 4 * Width;
            return Color.FromArgb(Data[idx + 3], Data[idx], Data[idx + 1], Data[idx + 2]);
        }
        set
        {
            var idx = x * 4 + y * 4 * Width;
            Data[idx] = value.R;
            Data[idx + 1] = value.G;
            Data[idx + 2] = value.B;
            Data[idx + 3] = value.A;
        }
    }

    public unsafe void Upload(uint texture)
    {
        fixed (byte* b = Data)
        {
            BfRendering.Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)Width,
                (uint)Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, b);
        }
    }
}