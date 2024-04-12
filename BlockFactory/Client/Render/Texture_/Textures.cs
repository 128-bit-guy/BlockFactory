using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using StbImageSharp;

namespace BlockFactory.Client.Render.Texture_;

[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class Textures
{
    public static TextureAtlas Blocks = null!;
    public static TextureAtlas Gui = null!;
    public static TextureAtlas Items = null!;

    public static void Init()
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        foreach (var name in typeof(Textures).Assembly.GetManifestResourceNames()) Console.WriteLine(name);
        Blocks = LoadTextureAtlas("BlockFactory.Assets.Textures.Blocks.png", 4);
        Gui = LoadTextureAtlas("BlockFactory.Assets.Textures.Gui.png", 2);
        Items = LoadTextureAtlas("BlockFactory.Assets.Textures.Items.png", 4);
    }

    public static Texture LoadTexture(string location)
    {
        using var stream = BlockFactoryClient.ResourceLoader.GetResourceStream(location);
        var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var img = new Image(result);
        return new Texture(img);
    }

    public static TextureAtlas LoadTextureAtlas(string location, int sizeLog2)
    {
        using var stream = BlockFactoryClient.ResourceLoader.GetResourceStream(location);
        var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var img = new Image(result);
        return new TextureAtlas(img, sizeLog2);
    }

    public static void Destroy()
    {
        Blocks.Dispose();
        Gui.Dispose();
        Items.Dispose();
    }
}