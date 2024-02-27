using System.Drawing;
using BlockFactory.Base;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public class MeshBuilder<T> where T : unmanaged
{
    public readonly MatrixStack Matrices;
    public readonly IUvTransformer UvTransformer;
    private uint _indBegin;
    private uint[] _indices;
    private int _vertCnt;
    private T[] _vertices;
    public Color Color = Color.White;

    public MeshBuilder(MatrixStack matrices, IUvTransformer transformer)
    {
        _vertices = new T[1];
        _indices = new uint[1];
        Matrices = matrices;
        UvTransformer = transformer;
    }

    public MeshBuilder(IUvTransformer transformer) : this(new MatrixStack(), transformer)
    {
    }

    public MeshBuilder() : this(IdentityUvTransformer.Instance)
    {
    }

    public int IndexCount { get; private set; }

    public MeshBuilder<T> NewPolygon()
    {
        _indBegin = (uint)_vertCnt;
        return this;
    }

    public MeshBuilder<T> Vertex(T vertex)
    {
        if (_vertCnt == _vertices.Length)
        {
            var verts = _vertices;
            _vertices = new T[2 * verts.Length];
            Array.Copy(verts, _vertices, verts.Length);
        }

        VertexTransformationInfo<T>.Transformer(ref vertex, this);
        _vertices[_vertCnt++] = vertex;
        return this;
    }

    public MeshBuilder<T> Index(uint index)
    {
        if (IndexCount == _indices.Length)
        {
            var inds = _indices;
            _indices = new uint[2 * inds.Length];
            Array.Copy(inds, _indices, inds.Length);
        }

        _indices[IndexCount++] = _indBegin + index;
        return this;
    }

    public MeshBuilder<T> Indices(params uint[] indices)
    {
        foreach (var index in indices) Index(index);

        return this;
    }

    public MeshBuilder<T> Upload(RenderMesh mesh)
    {
        mesh.Upload<T>(_vertices.AsSpan()[.._vertCnt], _indices.AsSpan()[..IndexCount]);
        return this;
    }

    public MeshBuilder<T> Reset()
    {
        _vertCnt = IndexCount = 0;
        _indBegin = 0;
        Color = Color.White;
        return this;
    }
}