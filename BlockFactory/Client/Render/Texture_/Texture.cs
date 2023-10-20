using BlockFactory.Base;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render.Texture_;

[ExclusiveTo(Side.Client)]
public class Texture : IDisposable
{
    private readonly uint _id;

    public Texture(Image image, TextureWrapMode mode = TextureWrapMode.Repeat, int maxLevel = -1)
    {
        _id = BfRendering.Gl.GenTexture();
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, _id);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)mode);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)mode);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapLinear);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        if (maxLevel != -1)
            BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, maxLevel);
        image.Upload(_id);
        BfRendering.Gl.GenerateMipmap(TextureTarget.Texture2D);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        BfRendering.Gl.DeleteTexture(_id);
    }

    public void Bind()
    {
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, _id);
    }
}