using BlockFactory.Base;
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
}