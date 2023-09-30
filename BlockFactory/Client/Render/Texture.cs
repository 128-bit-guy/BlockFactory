using BlockFactory.Base;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class Texture : IDisposable
{
    private readonly uint _id;

    public Texture(Image image)
    {
        _id = BfRendering.Gl.GenTexture();
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, _id);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.Repeat);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.Repeat);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapLinear);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 
            (int)TextureMagFilter.Nearest);
        image.Upload(_id);
        BfRendering.Gl.GenerateMipmap(TextureTarget.Texture2D);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind()
    {
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, _id);
    }

    public void Dispose()
    {
        BfRendering.Gl.DeleteTexture(_id);
    }
}