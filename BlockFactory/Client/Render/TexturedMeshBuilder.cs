using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class TexturedMeshBuilder
{
    public readonly MeshBuilder<BlockVertex> MeshBuilder;
    public readonly TextureAtlasUvTransformer UvTransformer;

    public TexturedMeshBuilder(MatrixStack? matrices = null, TextureAtlas? atlas = null)
    {
        UvTransformer = new TextureAtlasUvTransformer(atlas ?? Textures.Items);
        MeshBuilder = new MeshBuilder<BlockVertex>(matrices ?? new MatrixStack(), UvTransformer, FullBrightLightTransformer.Instance);
    }

    public void Reset()
    {
        MeshBuilder.Reset();
        MeshBuilder.Matrices.Reset();
    }
}