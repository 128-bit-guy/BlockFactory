using BlockFactory.Base;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render.Texture_;

[ExclusiveTo(Side.Client)]
public class TextureAtlas : IDisposable
{
    private readonly Box2D<float>[] _spriteBoxes;
    private readonly Texture _texture;

    public readonly int SizeLog2;

    public TextureAtlas(Image image, int sizeLog2)
    {
        SizeLog2 = sizeLog2;
        var spriteW = image.Width >> sizeLog2;
        var spriteH = image.Height >> sizeLog2;
        var nImage = new Image(image.Width << 1, image.Height << 1);
        _spriteBoxes = new Box2D<float>[1 << (sizeLog2 << 1)];
        for (var i = 0; i < 1 << sizeLog2; ++i)
        for (var j = 0; j < 1 << sizeLog2; ++j)
        {
            var minSx = ((i << 1) | 1) * spriteW;
            var maxSx = ((i + 1) << 1) * spriteW;
            var minSy = ((j << 1) | 1) * spriteH;
            var maxSy = ((j + 1) << 1) * spriteH;
            var maxPx = Math.Min((((i + 1) << 1) | 1) * spriteW - (spriteW >> 1), nImage.Width);
            var maxPy = Math.Min((((j + 1) << 1) | 1) * spriteH - (spriteH >> 1), nImage.Height);
            for (var x = ((i << 1) | 1) * spriteW - (spriteW >> 1);
                 x < maxPx;
                 ++x)
            for (var y = ((j << 1) | 1) * spriteH - (spriteH >> 1);
                 y < maxPy;
                 ++y)
            {
                var clampedX = Math.Clamp(x, minSx, maxSx - 1);
                var clampedY = Math.Clamp(y, minSy, maxSy - 1);
                var ox = clampedX - ((i << 1) | 1) * spriteW + i * spriteW;
                var oy = clampedY - ((j << 1) | 1) * spriteH + j * spriteH;
                nImage[x, y] = image[ox, oy];
            }

            _spriteBoxes[i | (((1 << sizeLog2) - j - 1) << sizeLog2)] = new Box2D<float>(
                (float)minSx / nImage.Width, (float)minSy / nImage.Height,
                (float)maxSx / nImage.Width, (float)maxSy / nImage.Height);
        }

        _texture = new Texture(nImage, TextureWrapMode.ClampToEdge, 4);
    }

    public void Dispose()
    {
        _texture.Dispose();
    }

    public Box2D<float> GetSpriteBox(int sprite)
    {
        return _spriteBoxes[sprite];
    }

    public void Bind()
    {
        _texture.Bind();
    }
}