using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class TextureArrayManager
{
    private readonly Dictionary<string, int> _imageIndices;
    private readonly List<ImageResult> Images;
    private int Arr;

    public TextureArrayManager()
    {
        Images = new List<ImageResult>();
        Arr = -1;
        _imageIndices = new Dictionary<string, int>();
    }

    public int AddOrGetImage(string location)
    {
        if (_imageIndices.TryGetValue(location, out var value))
            return value;
        return _imageIndices[location] = AddImage(location);
    }

    public int AddImage(string location)
    {
        var result = Images.Count;
        ImageResult? imgResult = null;
        using (var stream = ResourceLoader.GetResourceStream(location)!)
        {
            Images.Add(imgResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha));
        }

        if (imgResult.Width != Images[0].Width || imgResult.Height != Images[0].Height)
            throw new ArgumentException(string.Format("Image \"{0}\" size is not equal to first image size", location));
        return result;
    }

    public void Upload()
    {
        if (Arr != -1) GL.DeleteTexture(Arr);
        Arr = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, Arr);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 4, SizedInternalFormat.Rgba8, Images[0].Width, Images[0].Height,
            Images.Count);
        for (var i = 0; i < Images.Count; ++i)
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, Images[0].Width, Images[0].Height, 1,
                PixelFormat.Rgba, PixelType.UnsignedByte, Images[i].Data);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
        GL.BindTexture(TextureTarget.Texture2DArray, 0);
        Images.Clear();
        GLPreconditions.CheckGLError();
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2DArray, Arr);
    }
}