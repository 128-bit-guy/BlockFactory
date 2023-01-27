using BlockFactory.Client;
using BlockFactory.Resource;
using BlockFactory.Side_;

namespace BlockFactory.Render;

[ExclusiveTo(Side.Client)]
public class Textures : IDisposable
{
    public readonly Texture2D Dirt;
    public readonly TextureArrayManager BlockArray;
    public readonly int DirtLayer;
    
    public Textures(BlockFactoryClient client)
    {
        var resourceLoader = client.ResourceLoader;
        Dirt = new Texture2D("BlockFactory.Assets.Textures.dirt.png", resourceLoader);
        BlockArray = new TextureArrayManager(resourceLoader);
        DirtLayer = BlockArray.AddOrGetImage("BlockFactory.Assets.Textures.dirt.png");
        BlockArray.Upload();
    }

    public void Dispose()
    {
        Dirt.Dispose();
        BlockArray.Dispose();
    }
}