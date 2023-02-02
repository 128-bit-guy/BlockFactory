using System.Reflection;
using System.Runtime.InteropServices;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Mesh;

[ExclusiveTo(Side.Client)]
public class VertexFormat<T>
    where T : struct
{
    public delegate T VertexFloatTransformer(T a, float b);

    public delegate T VertexMatrixTransformer(T a, Matrix4 m);

    public delegate T VertexVector3Transformer(T a, Vector3 b);

    public readonly VertexVector3Transformer Colorer;
    public readonly VertexFloatTransformer LayerSetter;
    public readonly VertexMatrixTransformer MatrixApplier;
    public List<(int layoutLocation, int offset, int count, VertexAttribPointerType type)> VertexAttributeOffsets;

    public VertexFormat(VertexMatrixTransformer matrixApplier, VertexVector3Transformer colorer,
        VertexFloatTransformer layerSetter)
    {
        MatrixApplier = matrixApplier;
        Colorer = colorer;
        LayerSetter = layerSetter;
        VertexAttributeOffsets = new List<(int, int, int, VertexAttribPointerType)>();
        var t = typeof(T);
        Size = Marshal.SizeOf<T>();
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var el = f.GetCustomAttribute<VertexAttributeAttribute>();
            if (el != null)
            {
                var (count, type) = GLTypes.GetVertexAttribType(f.FieldType);
                VertexAttributeOffsets.Add((el.LayoutLocation, Marshal.OffsetOf<T>(f.Name).ToInt32(), count, type));
            }
            else
            {
                throw new ArgumentException(string.Format("Type {0} contains field {1}, which does not have {2}",
                    t.Name, f.Name, typeof(VertexAttributeAttribute).Name));
            }
        }
    }

    public int Size { get; }

    public void Enable(int VAO, int VBO, int bindingIndex)
    {
        GL.VertexArrayVertexBuffer(VAO, bindingIndex, VBO, new IntPtr(0), Size);
        foreach (var (layoutLocation, offset, count, type) in VertexAttributeOffsets)
        {
            GL.EnableVertexArrayAttrib(VAO, layoutLocation);
            GL.VertexArrayAttribFormat(VAO, layoutLocation, count, (VertexAttribType)type, false, offset);
            GL.VertexArrayAttribBinding(VAO, layoutLocation, bindingIndex);
        }
    }
}