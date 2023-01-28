using OpenTK.Graphics.OpenGL4;

namespace BlockFactory.Client.Render.Mesh;

public class RenderMesh<T>
        where T : struct
    {
        private readonly VertexFormat<T> Format;
        private int VAO;
        private int VBO;
        private int IBO;
        public int IndexCount { get; private set; }

        public RenderMesh(VertexFormat<T> format)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
            VAO = VBO = IBO = -1;
            IndexCount = 0;
        }

        public void Upload(int vertexCount, T[] vertices, int indexCount, int[] indices)
        {
            if (indexCount == 0 && VAO != -1)
            {
                IndexCount = 0;
                GL.DeleteVertexArray(VAO);
                GL.DeleteBuffers(2, new int[] { VBO, IBO });
                VAO = VBO = IBO = -1;
            }
            else if (indexCount > 0)
            {
                if (VAO == -1)
                {
                    GL.CreateVertexArrays(1, out VAO);
                    GL.CreateBuffers(1, out VBO);
                    GL.CreateBuffers(1, out IBO);
                    Format.Enable(VAO, VBO, 0);
                    GL.VertexArrayElementBuffer(VAO, IBO);
                }
                GL.NamedBufferData(VBO, Format.Size * vertexCount, vertices, BufferUsageHint.StaticDraw);
                GL.NamedBufferData(IBO, sizeof(int) * indexCount, indices, BufferUsageHint.StaticDraw);
                IndexCount = indexCount;
            }
        }

        public void Upload(T[] vertices, int[] indices)
        {
            Upload(vertices.Length, vertices, indices.Length, indices);
        }

        public void Bind()
        {
            if (VAO != -1)
            {
                GL.BindVertexArray(VAO);
            }
        }

        public void DeleteGl()
        {
            if (VAO != -1)
            {
                GL.DeleteVertexArray(VAO);
                GL.DeleteBuffer(VBO);
                GL.DeleteBuffer(IBO);
                VAO = VBO = IBO = -1;
                IndexCount = 0;
            }
        }
    }