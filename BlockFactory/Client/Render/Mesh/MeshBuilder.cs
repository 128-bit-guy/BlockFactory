using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Mesh;

public class MeshBuilder<T>
        where T : struct
    {
        private T[] Vertices;
        private int[] Indices;
        public int VertexCount { get; private set; }
        public int IndexCount { get; private set; }
        private int CurrentIndexSpace;
        private readonly VertexFormat<T> Format;
        public Vector3 Color;
        public float Layer;
        public readonly MatrixStack MatrixStack;
        public MeshBuilder(VertexFormat<T> format)
        {
            Color = (1, 1, 1);
            Layer = 0f;
            Format = format;
            Vertices = new T[0];
            Indices = new int[0];
            VertexCount = 0;
            IndexCount = 0;
            CurrentIndexSpace = -1;
            MatrixStack = new MatrixStack();
        }

        public void BeginIndexSpace()
        {
            if (CurrentIndexSpace != -1)
            {
                throw new InvalidOperationException("Index space already began");
            }
            else
            {
                CurrentIndexSpace = VertexCount;
            }
        }

        public void EndIndexSpace()
        {
            if (CurrentIndexSpace == -1)
            {
                throw new InvalidOperationException("Index space did not begin");
            }
            else
            {
                CurrentIndexSpace = -1;
            }
        }

        public void AddIndex(int i)
        {
            if (CurrentIndexSpace == -1)
            {
                throw new InvalidOperationException("Index space did not begin");
            }
            else
            {
                if (IndexCount == Indices.Length)
                {
                    GrowIndices();
                }
                Indices[IndexCount] = CurrentIndexSpace + i;
                ++IndexCount;
            }
        }

        public void AddIndices(params int[] indices)
        {
            foreach (int i in indices)
            {
                AddIndex(i);
            }
        }

        public void AddVertex(T v)
        {
            if (CurrentIndexSpace == -1)
            {
                throw new InvalidOperationException("Index space did not begin");
            }
            else
            {
                if (VertexCount == Vertices.Length)
                {
                    GrowVertices();
                }
                Vertices[VertexCount] = Format.LayerSetter(Format.Colorer(Format.MatrixApplier(v, MatrixStack), Color), Layer);
                ++VertexCount;
            }

        }

        public void AddVertices(T[] vertices)
        {
            foreach (T v in vertices)
            {
                AddVertex(v);
            }
        }

        public void Upload(RenderMesh<T> m)
        {

            if (CurrentIndexSpace != -1)
            {
                throw new InvalidOperationException("Index space is not finished");
            }
            else
            {
                m.Upload(VertexCount, Vertices, IndexCount, Indices);
            }
        }

        public void Reset()
        {
            if (CurrentIndexSpace != -1)
            {
                throw new InvalidOperationException("Index space is not finished");
            }
            else
            {
                IndexCount = 0;
                VertexCount = 0;
                MatrixStack.Reset();
                Color = (1, 1, 1);
                Layer = 0f;
            }
        }

        public void Clear()
        {
            Reset();
            Indices = new int[0];
            Vertices = new T[0];
        }

        private void GrowVertices()
        {
            if (Vertices.Length == 0)
            {
                Vertices = new T[1];
            }
            else
            {
                T[] oldVertices = Vertices;
                Vertices = new T[2 * oldVertices.Length];
                Array.Copy(oldVertices, Vertices, oldVertices.Length);
            }
        }

        private void GrowIndices()
        {
            if (Indices.Length == 0)
            {
                Indices = new int[1];
            }
            else
            {
                int[] oldIndices = Indices;
                Indices = new int[2 * oldIndices.Length];
                Array.Copy(oldIndices, Indices, oldIndices.Length);
            }
        }
    }