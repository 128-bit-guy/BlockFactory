using BlockFactory.Client.Render.Mesh_;
using BlockFactory.CubeMath;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

public class SkyRenderer : IDisposable
{
    private static readonly uint[] QuadIndices = { 0, 2, 1, 0, 3, 2 };
    private RenderMesh _skyboxCube;

    public SkyRenderer()
    {
        var builder = new MeshBuilder<PosVertex>();
        builder.Matrices.Push();
        builder.Matrices.Scale(10);
        builder.Matrices.Translate(-0.5f, -0.5f, -0.5f);
        foreach (var face in CubeFaceUtils.Values())
        {
            var s = face.GetAxis() == 1
                ? CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0]
                : CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;


            builder.Matrices.Push();
            builder.Matrices.Multiply(s.AroundCenterMatrix4);
            builder.NewPolygon().Indices(QuadIndices)
                .Vertex(new PosVertex(0, 0, 1))
                .Vertex(new PosVertex(1, 0, 1))
                .Vertex(new PosVertex(1, 1, 1))
                .Vertex(new PosVertex(0, 1, 1));
            builder.Matrices.Pop();
        }
        builder.Matrices.Pop();

        _skyboxCube = new RenderMesh();
        builder.Upload(_skyboxCube);
    }
    public unsafe void UpdateAndRender()
    {
        Shaders.Sky.Use();
        BfRendering.SetVpMatrices(Shaders.Sky);
        BfRendering.Matrices.Push();
        Shaders.Sky.SetModel(BfRendering.Matrices);
        _skyboxCube.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _skyboxCube.IndexCount,
            DrawElementsType.UnsignedInt, null);
        BfRendering.Matrices.Pop();
    }
    public void Dispose()
    {
        _skyboxCube.Dispose();
    }
}