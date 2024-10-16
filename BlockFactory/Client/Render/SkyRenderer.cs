using System.Numerics;
using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.CubeMath;
using BlockFactory.Utils;
using BlockFactory.Utils.Random_;
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
    private TexturedMeshBuilder _skyObjectBuilder;
    private readonly uint _fbo;
    public readonly uint Texture;
    private readonly LinearCongruentialRandom _rng = new();
    private readonly List<Vector2D<float>> _cloudPoses = new();
    private readonly List<Vector2D<float>> _cloudsSorted = new();
    private const float CloudMaxAlphaDist = 150;
    private const float CloudMinAlphaDist = 200;
    private const float CloudHeight = 100;
    private const float CloudSpeed = 4;
    private const int InitialCloudCount = 40;
    private const double CloudSpawnSeconds = 5;
    private static Comparison<Vector2D<float>> _cloudComparison = CloudComparison;
    private double _timeSinceCloudSpawn;

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
        _skyObjectBuilder = new TexturedMeshBuilder(null, Textures.Sky);

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
        for (var i = 0; i < InitialCloudCount; ++i)
        {
            var x = ((float)Random.Shared.NextDouble() * 2 - 1) * CloudMinAlphaDist;
            var z = ((float)Random.Shared.NextDouble() * 2 - 1) * CloudMinAlphaDist;
            _cloudPoses.Add(new Vector2D<float>(x, z));
        }
    }

    private void UpdateClouds(double deltaTime)
    {
        var movement = (float)deltaTime * CloudSpeed;
        for (var i = 0; i < _cloudPoses.Count; ++i)
        {
            _cloudPoses[i] += new Vector2D<float>(movement, 0);
        }

        _cloudPoses.RemoveAll(pos => pos.X > CloudMinAlphaDist);

        _timeSinceCloudSpawn += deltaTime;
        while (_timeSinceCloudSpawn > CloudSpawnSeconds)
        {
            _timeSinceCloudSpawn -= CloudSpawnSeconds;
            var z = ((float)Random.Shared.NextDouble() * 2 - 1) * CloudMinAlphaDist;
            _cloudPoses.Add(new Vector2D<float>(-CloudMinAlphaDist, z));
        }
    }

    protected void RenderSkyObjects(double deltaTime)
    {
        var builder = _skyObjectBuilder.MeshBuilder;
        builder.Matrices.Push();
        builder.Matrices.Push();
        builder.Matrices.RotateX(BlockFactoryClient.Player!.World!.WorldTimeManager.GetSunAngle());
        _rng.SetSeed(0);
        _skyObjectBuilder.UvTransformer.Sprite = 2;
        var starVisibility = 1 - BlockFactoryClient.Player.World!.GetDayCoefficient();
        builder.Color = new Vector4D<float>(1, 1, 1, starVisibility);
        for (var i = 0; i < 500; ++i)
        {
            var dir = RandomUtils.PointOnSphere(_rng);
            var size = (float)_rng.NextDouble() / 500 + 1.0f / 500;
            var angle = (float)_rng.NextDouble() * 2 * MathF.PI;
            var quat = BfMathUtils.GetFromToQuaternion(new Vector3D<float>(0, 0, 1), dir);
            builder.Matrices.Push();
            builder.Matrices.Rotate(quat);
            builder.Matrices.RotateZ(angle);
            builder.Matrices.Scale(size, size, 1);
            builder.NewPolygon().Indices(InvertedQuadIndices)
                .Vertex(new BlockVertex(-1, -1, 1, 1, 1, 1, 1, 0, 1))
                .Vertex(new BlockVertex(1, -1, 1, 1, 1, 1, 1, 1, 1))
                .Vertex(new BlockVertex(1, 1, 1, 1, 1, 1, 1, 1, 0))
                .Vertex(new BlockVertex(-1, 1, 1, 1, 1, 1, 1, 0, 0));
            builder.Matrices.Pop();
        }
        builder.Color = new Vector4D<float>(1, 1, 1, 1);
        _skyObjectBuilder.UvTransformer.Sprite = 0;
        builder.Matrices.Push();
        builder.Matrices.Scale(0.2f, 0.2f, 1.0f);
        builder.NewPolygon().Indices(InvertedQuadIndices)
            .Vertex(new BlockVertex(-1, -1, 1, 1, 1, 1, 1, 0, 1))
            .Vertex(new BlockVertex(1, -1, 1, 1, 1, 1, 1, 1, 1))
            .Vertex(new BlockVertex(1, 1, 1, 1, 1, 1, 1, 1, 0))
            .Vertex(new BlockVertex(-1, 1, 1, 1, 1, 1, 1, 0, 0));
        builder.Matrices.Pop();
        _skyObjectBuilder.UvTransformer.Sprite = 1;
        builder.Matrices.Push();
        builder.Matrices.Scale(0.15f, 0.15f, 1.0f);
        builder.Matrices.RotateX(MathF.PI);
        builder.NewPolygon().Indices(InvertedQuadIndices)
            .Vertex(new BlockVertex(-1, -1, 1, 1, 1, 1, 1, 0, 1))
            .Vertex(new BlockVertex(1, -1, 1, 1, 1, 1, 1, 1, 1))
            .Vertex(new BlockVertex(1, 1, 1, 1, 1, 1, 1, 1, 0))
            .Vertex(new BlockVertex(-1, 1, 1, 1, 1, 1, 1, 0, 0));
        builder.Matrices.Pop();
        builder.Matrices.Pop();
        UpdateClouds(deltaTime);
        _skyObjectBuilder.UvTransformer.Sprite = 3;
        _cloudsSorted.AddRange(_cloudPoses);
        _cloudsSorted.Sort(_cloudComparison);
        var cloudBrightness = BlockFactoryClient.Player.World!.GetDayCoefficient();
        foreach (var pos in _cloudsSorted)
        {
            var pos3D = new Vector3D<float>(pos.X, CloudHeight, pos.Y);
            var visibility = 1 - (pos.Length - CloudMaxAlphaDist) / (CloudMinAlphaDist - CloudMaxAlphaDist);
            visibility = MathF.Min(1, MathF.Max(0, visibility));
            builder.Color = new Vector4D<float>(cloudBrightness, cloudBrightness, cloudBrightness, visibility);
            builder.Matrices.Push();
            builder.Matrices.Translate(pos3D);
            builder.Matrices.RotateY(BlockFactoryClient.Player.HeadRotation.X + MathF.PI);
            builder.Matrices.RotateX(BlockFactoryClient.Player.HeadRotation.Y + MathF.PI);
            builder.Matrices.Scale(30);
            builder.NewPolygon().Indices(InvertedQuadIndices)
                .Vertex(new BlockVertex(-1, -1, 0, 1, 1, 1, 1, 0, 1))
                .Vertex(new BlockVertex(1, -1, 0, 1, 1, 1, 1, 1, 1))
                .Vertex(new BlockVertex(1, 1, 0, 1, 1, 1, 1, 1, 0))
                .Vertex(new BlockVertex(-1, 1, 0, 1, 1, 1, 1, 0, 0));
            builder.Matrices.Pop();
        }

        builder.Color = new Vector4D<float>(1, 1, 1, 1);
        _cloudsSorted.Clear();
        builder.Matrices.Pop();
    }

    private static int CloudComparison(Vector2D<float> a, Vector2D<float> b)
    {
        var aKey = a.LengthSquared;
        var bKey = b.LengthSquared;
        return -aKey.CompareTo(bKey);
    }

    protected unsafe void RenderSky(double deltaTime)
    {
        if (BlockFactoryClient.RenderWireframe)
        {
            BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }
        Shaders.Sky.Use();
        BfRendering.SetVpMatrices(Shaders.Sky);
        BfRendering.Matrices.Push();
        Shaders.Sky.SetModel(BfRendering.Matrices);
        Shaders.Sky.SetSunDirection(BlockFactoryClient.Player!.World!.WorldTimeManager.GetSunDirection());
        Shaders.Sky.SetDayCoef(BlockFactoryClient.Player!.World!.WorldTimeManager.GetDayCoefficient());
        _skyboxCube.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _skyboxCube.IndexCount,
            DrawElementsType.UnsignedInt, null);
        if (BlockFactoryClient.RenderWireframe)
        {
            BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        }
        RenderSkyObjects(deltaTime);
        RenderSkyObjectMesh();
        BfRendering.Matrices.Pop();
    }

    private unsafe void RenderSkyObjectMesh()
    {
        _skyObjectBuilder.MeshBuilder.Upload(_celestialMesh);

        _skyObjectBuilder.MeshBuilder.Reset();

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

    public unsafe void UpdateAndRender(double deltaTime)
    {
        BfRendering.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        RenderSky(deltaTime);
        if (BlockFactoryClient.RenderWireframe)
        {
            BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }
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
        if (BlockFactoryClient.RenderWireframe)
        {
            BfRendering.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        }
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