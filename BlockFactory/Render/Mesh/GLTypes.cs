using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Render.Mesh;

[ExclusiveTo(Side.Client)]
public static class GLTypes
{
    public static (int count, VertexAttribPointerType type) GetVertexAttribType(Type t)
    {
        if (t == typeof(Vector3))
            return (3, VertexAttribPointerType.Float);
        if (t == typeof(float))
            return (1, VertexAttribPointerType.Float);
        if (t == typeof(Vector2))
            return (2, VertexAttribPointerType.Float);
        if (t == typeof(Vector4))
            return (4, VertexAttribPointerType.Float);
        throw new ArgumentException(string.Format("Type {0} is not GL type", t.Name));
    }
}