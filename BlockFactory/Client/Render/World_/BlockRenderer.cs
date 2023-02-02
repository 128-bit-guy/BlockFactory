using BlockFactory.Block_;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.CubeMath;
using BlockFactory.Side_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.World_;

[ExclusiveTo(Side.Client)]
public class BlockRenderer
{
    public readonly WorldRenderer Renderer;

    public BlockRenderer(WorldRenderer renderer)
    {
        Renderer = renderer;
    }

    public void RenderBlock(BlockState state, IBlockReader reader, Vector3i pos, MeshBuilder<BlockVertex> mb)
    {
        var blockRotation = state.Rotation;
        var mesher = BlockMeshing.Meshers[state.Block];
        foreach (var direction in DirectionUtils.GetValues())
        {
            var layer = mesher.GetFaceTexture(reader, pos, state, direction);
            if (layer.HasValue)
            {
                mb.Layer = layer.Value;
                var absoluteDirection = blockRotation * direction;
                var neighbourPos = pos + absoluteDirection.GetOffset();
                var neighbourState = reader.GetBlockState(neighbourPos);
                var otherMesher =
                    BlockMeshing.Meshers[neighbourState.Block];
                var oppositeDirection = absoluteDirection.GetOpposite();
                var oRelativeDirection = neighbourState.Rotation.Inverse * oppositeDirection;
                var otherSolid = otherMesher.IsSideSolid(reader, neighbourPos,
                    neighbourState,
                    oRelativeDirection);
                if (!otherSolid)
                {
                    CubeRotation rotation;
                    if (direction.GetAxis() == 1)
                        rotation = CubeRotation.GetFromTo(Direction.North, direction)[0];
                    else
                        rotation = CubeRotation.GetFromToKeeping(Direction.North, direction,
                            Direction.Up);

                    var light = 1.0f - absoluteDirection.GetAxis() * 0.1f;
                    mb.MatrixStack.Push();
                    mb.MatrixStack.Multiply((blockRotation * rotation).AroundCenterRotation);
                    mb.BeginIndexSpace();
                    mb.AddVertex((1f, 0f, 0f, light, light, light, 0f, 0f));
                    mb.AddVertex((1f, 0f, 1f, light, light, light, 1f, 0f));
                    mb.AddVertex((1f, 1f, 1f, light, light, light, 1f, 1f));
                    mb.AddVertex((1f, 1f, 0f, light, light, light, 0f, 1f));
                    mb.AddIndices(0, 2, 1, 0, 3, 2);
                    mb.EndIndexSpace();
                    mb.MatrixStack.Pop();
                }
            }
        }
    }
}