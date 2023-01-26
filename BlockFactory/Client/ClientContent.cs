using BlockFactory.Render;
using BlockFactory.Render.Mesh;
using BlockFactory.Render.Shader;
using BlockFactory.Side_;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public class ClientContent : IDisposable
{
    public readonly BlockFactoryClient Client;
    public readonly Shaders Shaders;
    public readonly TextRenderer TextRenderer;
    public readonly VertexFormats VertexFormats;

    public ClientContent(BlockFactoryClient client)
    {
        Client = client;
        Shaders = new Shaders(client.ResourceLoader);
        Shaders.Init();
        using var stream = client.ResourceLoader
            .GetResourceStream("BlockFactory.Assets.Fonts.LiberationSerif-Regular.ttf")!;
        TextRenderer = new TextRenderer(stream, 64);
        VertexFormats = new VertexFormats();
        VertexFormats.Init();
    }

    public void Dispose()
    {
        Shaders.Dispose();
        TextRenderer.Dispose();
    }
}