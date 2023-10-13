using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;

namespace BlockFactory.Client.Render.Block_;

public class BlockMeshBuilder
{
    public readonly MeshBuilder<BlockVertex> MeshBuilder;
    public readonly TextureAtlasUvTransformer UvTransformer;

    public BlockMeshBuilder()
    {
        UvTransformer = new TextureAtlasUvTransformer(Textures.Blocks);
        MeshBuilder = new MeshBuilder<BlockVertex>(UvTransformer);
    }
}