using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Texture_;

[ExclusiveTo(Side.Client)]
public class TextureAtlas : IDisposable
{
    private readonly Texture _texture;

    public readonly int SizeLog2;

    private readonly Box2D<float>[] _spriteBoxes;

    public TextureAtlas(Image image, int sizeLog2)
    {
        SizeLog2 = sizeLog2;
        var spriteW = image.Width >> sizeLog2;
        var spriteH = image.Height >> sizeLog2;
        var nImage = new Image((image.Width << 1) + spriteW, (image.Height << 1) + spriteH);
        _spriteBoxes = new Box2D<float>[1 << (sizeLog2 << 1)];
        for (var i = 0; i < (1 << sizeLog2); ++i)
        {
            for (var j = 0; j < (1 << sizeLog2); ++j)
            {
                var minSx = ((i << 1) | 1) * spriteW;
                var maxSx = ((i + 1) << 1) * spriteW;
                var minSy = ((j << 1) | 1) * spriteH;
                var maxSy = ((j + 1) << 1) * spriteH;
                for (var x = ((i << 1) | 1) * spriteW - (spriteW >> 1); x < (((i + 1) << 1) | 1) * spriteW - (spriteW >> 1); ++x)
                {
                    for (var y = ((j << 1) | 1) * spriteH - (spriteH >> 1); y < (((j + 1) << 1) | 1) * spriteH - (spriteH >> 1); ++y)
                    {
                        var clampedX = Math.Clamp(x, minSx, maxSx - 1);
                        var clampedY = Math.Clamp(y, minSy, maxSy - 1);
                        var ox = clampedX - ((i << 1) | 1) * spriteW + i * spriteW;
                        var oy = clampedY - ((j << 1) | 1) * spriteH + j * spriteH;
                        nImage[x, y] = image[ox, oy];
                    }
                }

                _spriteBoxes[i | (((1 << sizeLog2) - j - 1) << sizeLog2)] = new Box2D<float>(
                    (float)minSx / nImage.Width, (float)minSy / nImage.Height, 
                    (float)maxSx / nImage.Width, (float)maxSy / nImage.Height);
            }
        }

        _texture = new Texture(nImage);
    }

    public Box2D<float> GetSpriteBox(int sprite)
    {
        return _spriteBoxes[sprite];
    }

    public void Bind()
    {
        _texture.Bind();
    }

    public void Dispose()
    {
        _texture.Dispose();
    }
}