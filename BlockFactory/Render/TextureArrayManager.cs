using BlockFactory.Resource;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace BlockFactory.Render;

[ExclusiveTo(Side.Client)]
public class TextureArrayManager : IDisposable
{
    private readonly Dictionary<string, int> _imageIndices;
    private readonly List<ImageResult> _images;
    private readonly IResourceLoader _loader;
    private int _arr;

    public TextureArrayManager(IResourceLoader loader)
    {
        _images = new List<ImageResult>();
        _arr = -1;
        _imageIndices = new Dictionary<string, int>();
        _loader = loader;
    }

    public int AddOrGetImage(string location)
    {
        if (_imageIndices.TryGetValue(location, out var value))
            return value;
        return _imageIndices[location] = AddImage(location);
    }

    public int AddImage(string location)
    {
        var result = _images.Count;
        ImageResult? imgResult = null;
        using (var stream = _loader.GetResourceStream(location)!)
        {
            _images.Add(imgResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha));
        }

        if (imgResult.Width != _images[0].Width || imgResult.Height != _images[0].Height)
            throw new ArgumentException(string.Format("Image \"{0}\" size is not equal to first image size", location));

        return result;
    }

    public void Upload()
    {
        if (_arr != -1) GL.DeleteTexture(_arr);

        _arr = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, _arr);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 4, SizedInternalFormat.Rgba8, _images[0].Width,
            _images[0].Height, _images.Count);
        for (var i = 0; i < _images.Count; ++i)
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, _images[0].Width, _images[0].Height, 1,
                PixelFormat.Rgba, PixelType.UnsignedByte, _images[i].Data);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
        GL.BindTexture(TextureTarget.Texture2DArray, 0);
        _images.Clear();
        GLPreconditions.CheckGLError();
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2DArray, _arr);
    }

    public void Dispose()
    {
        
        GL.DeleteTexture(_arr);
    }
}