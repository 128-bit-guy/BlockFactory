using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Render.Mesh;

[ExclusiveTo(Side.Client)]
public class MeshBuilder<T>
    where T : struct
{
    private readonly VertexFormat<T> Format;
    public readonly MatrixStack MatrixStack;
    public Vector3 Color;
    private int CurrentIndexSpace;
    private int[] Indices;
    public float Layer;
    private T[] Vertices;

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

    public int VertexCount { get; private set; }
    public int IndexCount { get; private set; }

    public void BeginIndexSpace()
    {
        if (CurrentIndexSpace != -1)
            throw new InvalidOperationException("Index space already began");
        CurrentIndexSpace = VertexCount;
    }

    public void EndIndexSpace()
    {
        if (CurrentIndexSpace == -1)
            throw new InvalidOperationException("Index space did not begin");
        CurrentIndexSpace = -1;
    }

    public void AddIndex(int i)
    {
        if (CurrentIndexSpace == -1)
        {
            throw new InvalidOperationException("Index space did not begin");
        }

        if (IndexCount == Indices.Length) GrowIndices();

        Indices[IndexCount] = CurrentIndexSpace + i;
        ++IndexCount;
    }

    public void AddIndices(params int[] indices)
    {
        foreach (var i in indices) AddIndex(i);
    }

    public void AddVertex(T v)
    {
        if (CurrentIndexSpace == -1)
        {
            throw new InvalidOperationException("Index space did not begin");
        }

        if (VertexCount == Vertices.Length) GrowVertices();

        Vertices[VertexCount] =
            Format.LayerSetter(Format.Colorer(Format.MatrixApplier(v, MatrixStack), Color), Layer);
        ++VertexCount;
    }

    public void AddVertices(T[] vertices)
    {
        foreach (var v in vertices) AddVertex(v);
    }

    public void Upload(RenderMesh<T> m)
    {
        if (CurrentIndexSpace != -1)
            throw new InvalidOperationException("Index space is not finished");
        m.Upload(VertexCount, Vertices, IndexCount, Indices);
    }

    public void Reset()
    {
        if (CurrentIndexSpace != -1)
        {
            throw new InvalidOperationException("Index space is not finished");
        }

        IndexCount = 0;
        VertexCount = 0;
        MatrixStack.Reset();
        Color = (1, 1, 1);
        Layer = 0f;
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
            var oldVertices = Vertices;
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
            var oldIndices = Indices;
            Indices = new int[2 * oldIndices.Length];
            Array.Copy(oldIndices, Indices, oldIndices.Length);
        }
    }
}