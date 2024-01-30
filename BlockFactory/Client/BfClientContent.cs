using System.Diagnostics.CodeAnalysis;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Texture_;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class BfClientContent
{
    public static TextRenderer TextRenderer = null!;
    public static void Init()
    {
        Textures.Init();
        Shaders.Init();
        using var stream = BlockFactoryClient.ResourceLoader.GetResourceStream("BlockFactory.Fonts.LiberationSerif-Regular.ttf")!;
        TextRenderer = new TextRenderer(stream, 64);
    }

    public static void Destroy()
    {
        Textures.Destroy();
        Shaders.Destroy();
        TextRenderer.Dispose();
    }
}