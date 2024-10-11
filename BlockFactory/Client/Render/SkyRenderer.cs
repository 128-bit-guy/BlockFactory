using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.CubeMath;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class SkyRenderer : IDisposable
{
    private static readonly uint[] InvertedQuadIndices = { 0, 2, 1, 0, 3, 2 };
    private readonly RenderMesh _skyboxCube;
    private readonly RenderMesh _skyBackground;
    private readonly RenderMesh _celestialMesh;
    private TexturedMeshBuilder _celestialBuilder;
    private readonly uint _fbo;
    public readonly uint Texture;

    public unsafe SkyRenderer()
    {
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
                builder.NewPolygon().Indices(InvertedQuadIndices)
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

        {
            _skyBackground = new RenderMesh();
            var builder = new MeshBuilder<GuiVertex>();
            builder.Matrices.Push();
            builder.NewPolygon().Indices(InvertedQuadIndices)
                .Vertex(new GuiVertex(-1, 1, 0, 1, 1, 1, 1, 0, 1))
                .Vertex(new GuiVertex(1, 1, 0, 1, 1, 1, 1, 1, 1))
                .Vertex(new GuiVertex(1, -1, 0, 1, 1, 1, 1, 1, 0))
                .Vertex(new GuiVertex(-1, -1, 0, 1, 1, 1, 1, 0, 0));
            builder.Matrices.Pop();
            builder.Upload(_skyBackground);
        }

        _celestialMesh = new RenderMesh(VertexBufferObjectUsage.StreamDraw);
        _celestialBuilder = new TexturedMeshBuilder(null, Textures.Sky);

        _fbo = BfRendering.Gl.CreateFramebuffer();
        Texture = BfRendering.Gl.GenTexture();
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, Texture);
        var sz = BlockFactoryClient.Window.FramebufferSize;
        BfRendering.Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)sz.X,
            (uint)sz.Y, 0, PixelFormat.Rgb, PixelType.Byte, null);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);
        BfRendering.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
        BfRendering.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        BfRendering.Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, Texture, 0);
        if (BfRendering.Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new GlException("Framebuffer incomplete");
        }
        BfRendering.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);   
    }

    protected void RenderCelestials()
    {
        var builder = _celestialBuilder.MeshBuilder;
        _celestialBuilder.UvTransformer.Sprite = 0;
        builder.Matrices.Push();
        builder.Matrices.RotateX(BlockFactoryClient.Player!.World!.WorldTimeManager.GetSunAngle());
        builder.Matrices.Push();
        builder.Matrices.Scale(0.2f, 0.2f, 1.0f);
        builder.NewPolygon().Indices(InvertedQuadIndices)
            .Vertex(new BlockVertex(-1, -1, 1, 1, 1, 1, 1, 0, 0))
            .Vertex(new BlockVertex(1, -1, 1, 1, 1, 1, 1, 1, 0))
            .Vertex(new BlockVertex(1, 1, 1, 1, 1, 1, 1, 1, 1))
            .Vertex(new BlockVertex(-1, 1, 1, 1, 1, 1, 1, 0, 1));
        builder.Matrices.Pop();
        builder.Matrices.Pop();
    }

    protected unsafe void RenderSky()
    {
        // BfRendering.Gl.Clear(ClearBufferMask.ColorBufferBit);
        Shaders.Sky.Use();
        BfRendering.SetVpMatrices(Shaders.Sky);
        BfRendering.Matrices.Push();
        Shaders.Sky.SetModel(BfRendering.Matrices);
        Shaders.Sky.SetSunDirection(BlockFactoryClient.Player!.World!.WorldTimeManager.GetSunDirection());
        Shaders.Sky.SetDayCoef(BlockFactoryClient.Player!.World!.WorldTimeManager.GetDayCoefficient());
        _skyboxCube.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _skyboxCube.IndexCount,
            DrawElementsType.UnsignedInt, null);
        RenderCelestials();
        RenderCelestialMesh();
        BfRendering.Matrices.Pop();
    }

    private unsafe void RenderCelestialMesh()
    {
        _celestialBuilder.MeshBuilder.Upload(_celestialMesh);

        _celestialBuilder.MeshBuilder.Reset();

        if (_celestialMesh.IndexCount == 0) return;

        BfRendering.Gl.Enable(EnableCap.Blend);
        BfRendering.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Shaders.Block.Use();
        Textures.Sky.Bind();
        BfRendering.SetVpMatrices(Shaders.Block);
        Shaders.Block.SetModel(BfRendering.Matrices);
        Shaders.Block.SetLoadProgress(1);
        Shaders.Block.SetSpriteBoxesBinding(2);
        Textures.Sky.SpriteBoxesBuffer.Bind(2);
        _celestialMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _celestialMesh.IndexCount,
            DrawElementsType.UnsignedInt, null);
        BfRendering.Gl.Disable(EnableCap.Blend);
    }

    public unsafe void UpdateAndRender()
    {
        BfRendering.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        RenderSky();
        BfRendering.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, Texture);
        Shaders.Gui.Use();
        Shaders.Gui.SetModel(Matrix4X4<float>.Identity);
        Shaders.Gui.SetView(Matrix4X4<float>.Identity);
        Shaders.Gui.SetProjection(Matrix4X4<float>.Identity);
        _skyBackground.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _skyBackground.IndexCount,
            DrawElementsType.UnsignedInt, null);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public unsafe void OnFramebufferResize(Vector2D<int> newSize)
    {
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, Texture);
        BfRendering.Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)newSize.X,
            (uint)newSize.Y, 0, PixelFormat.Rgb, PixelType.Byte, null);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        _skyboxCube.Dispose();
        _celestialMesh.Dispose();
        _skyBackground.Dispose();
        BfRendering.Gl.DeleteFramebuffer(_fbo);
        BfRendering.Gl.DeleteTexture(Texture);
    }
}