using System.Buffers;
using BlockFactory.Base;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Math_;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public static class VertexFieldTransformers<T> where T : unmanaged
{
    public static Vector3D<float> TransformPosition(Vector3D<float> pos, MeshBuilder<T> builder)
    {
        return Vector3D.Transform(pos, builder.Matrices);
    }

    public static Vector2D<float> TransformUv(Vector2D<float> pos, MeshBuilder<T> builder)
    {
        return builder.UvTransformer.TransformUv(pos);
    }

    public static Vector4D<float> TransformColor(Vector4D<float> color, MeshBuilder<T> builder)
    {
        return color * builder.Color.AsVector();
    }

    public static int TransformSpriteIndex(int spriteIndex, MeshBuilder<T> builder)
    {
        if (builder.UvTransformer is TextureAtlasUvTransformer transformer)
        {
            return transformer.Sprite;
        }
        else
        {
            return 0;
        }
    }
}