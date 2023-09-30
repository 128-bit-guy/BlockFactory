using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class Textures
{
    public static Texture Stone;
    public static void Init()
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        Stone = LoadTexture("BlockFactory.Textures.Stone.png");
    }

    public static Texture LoadTexture(string location)
    {
        using var stream = BlockFactoryClient.ResourceLoader.GetResourceStream(location);
        var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var img = new Image(result);
        return new Texture(img);
    }

    public static void Destroy()
    {
        Stone.Dispose();
    }
}