using OpenTK.Mathematics;
using BlockFactory.Block_;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.CubeMath;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;

namespace BlockFactory.Client.Render.World_;

public class BlockRenderer
{
    public readonly WorldRenderer Renderer;

    public BlockRenderer(WorldRenderer renderer)
    {
        Renderer = renderer;
    }

    public void RenderBlock(BlockState state, IBlockReader reader, Vector3i pos, MeshBuilder<BlockVertex> mb)
    {
        CubeRotation blockRotation = state.Rotation;
        IBlockMesher mesher = BlockMeshing.Meshers[state.Block];
        foreach (var direction in DirectionUtils.GetValues())
        {
            int? layer = mesher.GetFaceTexture(reader, pos, state, direction);
            if (layer.HasValue)
            {
                mb.Layer = layer.Value;
                Direction absoluteDirection = blockRotation * direction;
                Vector3i neighbourPos = pos + absoluteDirection.GetOffset();
                BlockState neighbourState = reader.GetBlockState(neighbourPos);
                IBlockMesher otherMesher =
                    BlockMeshing.Meshers[neighbourState.Block];
                Direction oppositeDirection = absoluteDirection.GetOpposite();
                Direction oRelativeDirection = neighbourState.Rotation.Inverse * oppositeDirection;
                bool otherSolid = otherMesher.IsSideSolid(reader, neighbourPos,
                    neighbourState,
                    oRelativeDirection);
                if (!otherSolid)
                {
                    CubeRotation rotation;
                    if (direction.GetAxis() == 1)
                    {
                        rotation = CubeRotation.GetFromTo(Direction.North, direction)[0];
                    }
                    else
                    {
                        rotation = CubeRotation.GetFromToKeeping(Direction.North, direction,
                            Direction.Up);
                    }

                    float light = 1.0f - DirectionUtils.GetAxis(absoluteDirection) * 0.1f;
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