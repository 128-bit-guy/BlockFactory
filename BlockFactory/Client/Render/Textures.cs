using BlockFactory.Client.Render.Block_;
using BlockFactory.Side_;
using StbImageSharp;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public static class Textures
{
    public static TextureArrayManager BlockArray = null!;
    public static Texture2D DirtTexture = null!;
    public static Texture2D StoneTexture = null!;
    public static TextRenderer TextRenderer = null!;

    public static void Init()
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        BlockArray = new TextureArrayManager();
        foreach (var mesher in BlockMeshing.Meshers.RegisteredAttachments) mesher.AddTextures(BlockArray);
        BlockArray.Upload();
        DirtTexture = new Texture2D("BlockFactory.Assets.Textures.dirt.png");
        StoneTexture = new Texture2D("BlockFactory.Assets.Textures.stone.png");
        using var stream = ResourceLoader.GetResourceStream("BlockFactory.Assets.Fonts.LiberationSerif-Regular.ttf")!;
        TextRenderer = new TextRenderer(stream, 64);
    }
}