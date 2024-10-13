using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public class BlockMeshBuilder
{
    public readonly MeshBuilder<BlockVertex> MeshBuilder;
    public readonly TextureAtlasUvTransformer UvTransformer;
    public uint TransparentStart;

    public BlockMeshBuilder(MatrixStack? matrices, ILightTransformer lightTransformer)
    {
        UvTransformer = new TextureAtlasUvTransformer(Textures.Blocks);
        MeshBuilder = new MeshBuilder<BlockVertex>(matrices ?? new MatrixStack(), UvTransformer, lightTransformer);
        TransparentStart = 0;
    }

    public void Reset()
    {
        MeshBuilder.Reset();
        MeshBuilder.Matrices.Reset();
        TransparentStart = 0;
    }
}