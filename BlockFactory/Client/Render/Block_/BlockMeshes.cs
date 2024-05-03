using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Utils;
using BlockFactory.World_;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public static class BlockMeshes
{
    private static readonly uint[] QuadIndices = { 0, 1, 2, 0, 2, 3 };

    private static readonly Box3D<float> CenterBox = new(12 / 32.0f, 0.0f, 12 / 32.0f,
        20 / 32.0f, 1.0f, 20 / 32.0f);

    private static readonly Box3D<float>[] ConnectionBoxes = new[]
    {
        new Box3D<float>(
            14 / 32.0f, 21 / 32.0f, 20 / 32.0f, 18 / 32.0f, 27 / 32.0f, 1.0f
        ),
        new Box3D<float>(
            14 / 32.0f, 9 / 32.0f, 20 / 32.0f, 18 / 32.0f, 15 / 32.0f, 1.0f
        )
    };

    private static readonly Box3D<float> TorchBox = new(14 / 32.0f, 0.0f, 14 / 32.0f,
        18 / 32.0f, 24 / 32.0f, 18 / 32.0f);

    public static void RenderFence(FenceBlock fence, BlockPointer pointer, BlockMeshBuilder bmb)
    {
        var light = Math.Max(pointer.GetLight(LightChannel.Sky), pointer.GetLight(LightChannel.Block)) / 15.0f;
        RenderCuboid(bmb, CenterBox, fence.GetTexture(CubeFace.Top), light);
        foreach (var face in CubeFaceUtils.Values())
        {
            if (face.GetAxis() == 1) continue;
            if (!fence.Connects(pointer, face)) continue;
            var s = CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;
            foreach (var connectionBox in ConnectionBoxes)
            {
                var transformedBox = s.TransformAroundCenter(connectionBox);
                RenderCuboid(bmb, transformedBox, fence.GetTexture(face), light);
            }
        }
    }

    public static void RenderTorch(TorchBlock torch, BlockPointer pointer, BlockMeshBuilder bmb)
    {
        var hasBlock = false;
        var hasPlanks = false;
        foreach (var face in CubeFaceUtils.Values())
        {
            var n = pointer + face.GetDelta();
            if(!n.GetBlockObj().IsFaceSolid(face.GetOpposite())) continue;
            hasBlock = true;
            if(n.GetBlock() != Blocks.Planks.Id) continue;
            hasPlanks = true;
        }

        if (!hasBlock)
        {
            RenderTorchFace(torch, pointer, bmb, CubeFace.Bottom);
        }
        
        foreach (var face in CubeFaceUtils.Values())
        {
            var n = pointer + face.GetDelta();
            if(!n.GetBlockObj().IsFaceSolid(face.GetOpposite())) continue;
            if(hasPlanks && n.GetBlock() != Blocks.Planks.Id) continue;
            RenderTorchFace(torch, pointer, bmb, face);
        }
    }

    private static void RenderTorchFace(TorchBlock torch, BlockPointer pointer, BlockMeshBuilder bmb, CubeFace face)
    {
        Span<int> t = stackalloc int[6];
        foreach(var f in CubeFaceUtils.Values())
        {
            t[(int)f] = torch.GetTexture(f);
        }
        var light = Math.Max(pointer.GetLight(LightChannel.Sky), pointer.GetLight(LightChannel.Block)) / 15.0f;
        var matrices = bmb.MeshBuilder.Matrices;
        matrices.Push();
        if (face.GetAxis() != 1)
        {
            var s = CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;
            matrices.Multiply(s.AroundCenterMatrix4);
            matrices.Translate(0.5f, 0.5f, 0.8f);
            matrices.RotateX(-MathF.PI / 6);
            matrices.Translate(-0.5f, -0.5f, -0.5f);
            matrices.Multiply(s.Inverse.AroundCenterMatrix4);
        }
        else if (face == CubeFace.Top)
        {
            var s = CubeSymmetry.GetFromTo(CubeFace.Bottom, face, true)[0];
            matrices.Multiply(s.AroundCenterMatrix4);
        }
        RenderCuboid(bmb, TorchBox, t, light);
        matrices.Pop();
    }

    public static void RenderTallGrass(TallGrassBlock g, BlockPointer pointer, BlockMeshBuilder bmb)
    {
        var light = Math.Max(pointer.GetLight(LightChannel.Sky), pointer.GetLight(LightChannel.Block)) / 15.0f;
        var tex = g.GetTexture(CubeFace.Top);
        var builder = bmb.MeshBuilder;
        var transformer = bmb.UvTransformer;
        var matrices = builder.Matrices;
        transformer.Sprite = tex;
        var ext = 0.5f;
        var mi = 0.5f - ext;
        var ma = 0.5f + ext;
        foreach (var face in CubeFaceUtils.Horizontals())
        {
            var s = CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;
            matrices.Push();
            matrices.Multiply(s.AroundCenterMatrix4);
            builder.NewPolygon().Indices(QuadIndices);
            builder.Vertex(new BlockVertex(mi, 0, mi, light, light, light, 1, 0, 0));
            builder.Vertex(new BlockVertex(ma, 0, ma, light, light, light, 1, 1, 0));
            builder.Vertex(new BlockVertex(ma, 1, ma, light, light, light, 1, 1, 1));
            builder.Vertex(new BlockVertex(mi, 1, mi, light, light, light, 1, 0, 1));
            matrices.Pop();
        }
    }

    private static void RenderCuboid(BlockMeshBuilder bmb, Box3D<float> cube, int texture, float light)
    {
        Span<int> t = stackalloc int[6];
        t.Fill(texture);
        RenderCuboid(bmb, cube, t, light);
    }

    private static void RenderCuboid(BlockMeshBuilder bmb, Box3D<float> cube, Span<int> textures, float light)
    {
        var builder = bmb.MeshBuilder;
        var transformer = bmb.UvTransformer;
        Span<Vector2D<float>> uvs = stackalloc Vector2D<float>[4];
        uvs[0] = new Vector2D<float>(0, 0);
        uvs[1] = new Vector2D<float>(1, 0);
        uvs[2] = new Vector2D<float>(1, 1);
        uvs[3] = new Vector2D<float>(0, 1);
        foreach (var face in CubeFaceUtils.Values())
        {
            transformer.Sprite = textures[(int)face];
            var s = face.GetAxis() == 1
                ? CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0]
                : CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;

            builder.Matrices.Push();
            builder.NewPolygon().Indices(QuadIndices);
            foreach (var uv in uvs)
            {
                var vert = new Vector3D<float>(uv, 1);
                var rotVert = s.TransformAroundCenter(vert);
                var lerpVert = BfMathUtils.BoxLerp(cube, rotVert);
                var reverseRotVert = s.Inverse.TransformAroundCenter(lerpVert);
                var resUv = new Vector2D<float>(reverseRotVert.X, reverseRotVert.Y);
                builder.Vertex(
                    new BlockVertex(lerpVert, new Vector4D<float>(light, light, light, 1), resUv)
                );
            }

            builder.Matrices.Pop();
        }
    }
}