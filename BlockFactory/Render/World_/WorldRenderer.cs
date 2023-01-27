using BlockFactory.Client;
using BlockFactory.CubeMath;
using BlockFactory.Render.Mesh;
using BlockFactory.Render.Mesh.Vertex;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Render.World_;

[ExclusiveTo(Side.Client)]
public class WorldRenderer : IDisposable
{
    public readonly BlockFactoryClient Client;

    public readonly RenderMesh<BlockVertex> Block;

    public WorldRenderer(BlockFactoryClient client)
    {
        Client = client;
        Block = new RenderMesh<BlockVertex>(client.ClientContent.VertexFormats.Block);
        var meshBuilder = new MeshBuilder<BlockVertex>(client.ClientContent.VertexFormats.Block);
        foreach(var face in CubeFaceUtils.GetValues())
        {
            meshBuilder.MatrixStack.Push();
            meshBuilder.MatrixStack.Multiply(CubeRotation.GetFromTo(CubeFace.Bottom, face)[0].AroundCenterRotation);
            meshBuilder.BeginIndexSpace();
            meshBuilder.AddIndices(0, 1, 2, 0, 2, 3);
            meshBuilder.Layer = Client.ClientContent.Textures.DirtLayer;
            meshBuilder.Color = new Vector3(0.8f + CubeFaceUtils.GetAxis(face) * 0.2f);
            meshBuilder.AddVertex((0, 0, 0, 1, 1, 1, 0, 0));
            meshBuilder.AddVertex((1, 0, 0, 1, 1, 1, 1, 0));
            meshBuilder.AddVertex((1, 0, 1, 1, 1, 1, 1, 1));
            meshBuilder.AddVertex((0, 0, 1, 1, 1, 1, 0, 1));
            meshBuilder.EndIndexSpace();
            meshBuilder.MatrixStack.Pop();
        }
        meshBuilder.Upload(Block);
    }
    public Matrix4 CreatePerspective()
    {
        var aspectRatio = (float)Client.Window.Size.X / Client.Window.Size.Y;
        return Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, aspectRatio, 0.1f, 1000f);
    }

    public void UseWorldMatrices()
    {
        Vector3 playerPos = (10 * (float)Math.Sin(GLFW.GetTime() / 8), 4, 10 * (float)Math.Cos(GLFW.GetTime() / 8));
        var up = Vector3.Cross(Vector3.Cross(playerPos, Vector3.UnitY), playerPos).Normalized();
        Client.VpMatrices.View = Matrix4.LookAt(playerPos, Vector3.Zero, up);
        Client.VpMatrices.Projection = CreatePerspective();
    }

    public void UpdateAndRender()
    {
        UseWorldMatrices();
        Client.ClientContent.Textures.BlockArray.Bind();
        Client.ClientContent.Shaders.Block.Use();
        Client.VpMatrices.Set(Client.ClientContent.Shaders.Block);
        Client.ClientContent.Shaders.Block.SetModel(Matrix4.Identity);
        Block.Bind();
        GL.DrawElements(PrimitiveType.Triangles, Block.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose()
    {
        Block.Dispose();
    }
}