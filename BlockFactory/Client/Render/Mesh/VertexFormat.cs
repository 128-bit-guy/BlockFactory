using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BlockFactory.Client.Render.Mesh;

public class VertexFormat<T>
        where T : struct
    {
        public int Size { get; private set; }
        public List<(int layoutLocation, int offset, int count, VertexAttribPointerType type)> VertexAttributeOffsets;
        public delegate T VertexVector3Transformer(T a, Vector3 b);
        public delegate T VertexFloatTransformer(T a, float b);
        public delegate T VertexMatrixTransformer(T a, Matrix4 m);
        public readonly VertexMatrixTransformer MatrixApplier;

        public readonly VertexVector3Transformer Colorer;
        public readonly VertexFloatTransformer LayerSetter;

        public unsafe VertexFormat(VertexMatrixTransformer matrixApplier, VertexVector3Transformer colorer, VertexFloatTransformer layerSetter)
        {
            MatrixApplier = matrixApplier;
            Colorer = colorer;
            LayerSetter = layerSetter;
            VertexAttributeOffsets = new List<(int, int, int, VertexAttribPointerType)>();
            Type t = typeof(T);
            Size = Marshal.SizeOf<T>();
            foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                VertexAttributeAttribute? el = f.GetCustomAttribute<VertexAttributeAttribute>();
                if (el != null)
                {
                    (int count, VertexAttribPointerType type) = GLTypes.GetVertexAttribType(f.FieldType);
                    VertexAttributeOffsets.Add((el.LayoutLocation, Marshal.OffsetOf<T>(f.Name).ToInt32(), count, type));
                }
                else
                {
                    throw new ArgumentException(string.Format("Type {0} contains field {1}, which does not have {2}", t.Name, f.Name, typeof(VertexAttributeAttribute).Name));
                }
            }
        }

        public void Enable(int VAO, int VBO, int bindingIndex)
        {
            GL.VertexArrayVertexBuffer(VAO, bindingIndex, VBO, new IntPtr(0), Size);
            foreach ((int layoutLocation, int offset, int count, VertexAttribPointerType type) in VertexAttributeOffsets)
            {
                GL.EnableVertexArrayAttrib(VAO, layoutLocation);
                GL.VertexArrayAttribFormat(VAO, layoutLocation, count, (VertexAttribType)type, false, offset);
                GL.VertexArrayAttribBinding(VAO, layoutLocation, bindingIndex);
            }
        }
    }