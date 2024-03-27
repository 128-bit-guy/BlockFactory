using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class ItemMeshBuilder
{
    public readonly MeshBuilder<BlockVertex> MeshBuilder;
    public readonly TextureAtlasUvTransformer UvTransformer;

    public ItemMeshBuilder()
    {
        UvTransformer = new TextureAtlasUvTransformer(Textures.Items);
        MeshBuilder = new MeshBuilder<BlockVertex>(UvTransformer);
    }

    public void Reset()
    {
        MeshBuilder.Reset();
        MeshBuilder.Matrices.Reset();
    }
}