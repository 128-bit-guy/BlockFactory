using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class Texture2D
{
    private ImageResult Image;
    private int Tex;

    public Texture2D()
    {
        Tex = -1;
    }

    public Texture2D(string location) : this()
    {
        SetImage(location);
        Upload();
        DeleteImage();
    }

    public void SetImage(string location)
    {
        using (var stream = ResourceLoader.GetResourceStream(location)!)
        {
            Image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }
    }

    public void Upload()
    {
        if (Tex != -1) GL.DeleteTexture(Tex);
        Tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Tex);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Image.Width, Image.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, Image.Data);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GLPreconditions.CheckGLError();
    }

    public void DeleteImage()
    {
        Image = null;
    }

    public void BindTexture()
    {
        GL.BindTexture(TextureTarget.Texture2D, Tex);
        //GL.BindTextureUnit(0, Tex);
    }
}