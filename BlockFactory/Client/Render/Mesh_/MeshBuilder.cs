using BlockFactory.Base;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public class MeshBuilder<T> where T : unmanaged
{
    private T[] _vertices;
    private int _vertCnt;
    private uint[] _indices;
    private int _indCnt;
    private uint _indBegin;
    public readonly MatrixStack Matrices;

    public MeshBuilder(MatrixStack matrices)
    {
        _vertices = new T[1];
        _indices = new uint[1];
        Matrices = matrices;
    }

    public MeshBuilder() : this(new MatrixStack())
    {
        
    }

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
        if (_indCnt == _indices.Length)
        {
            var inds = _indices;
            _indices = new uint[2 * inds.Length];
            Array.Copy(inds, _indices, inds.Length);
        }

        _indices[_indCnt++] = _indBegin + index;
        return this;
    }

    public MeshBuilder<T> Indices(params uint[] indices)
    {
        foreach (var index in indices)
        {
            Index(index);
        }

        return this;
    }

    public MeshBuilder<T> Upload(RenderMesh mesh)
    {
        mesh.Upload<T>(_vertices.AsSpan()[.._vertCnt], _indices.AsSpan()[.._indCnt]);
        return this;
    }

    public MeshBuilder<T> Reset()
    {
        _vertCnt = _indCnt = 0;
        _indBegin = 0;
        return this;
    }
}