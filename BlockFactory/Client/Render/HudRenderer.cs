using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Client.Render.Shader;
using BlockFactory.Client.World_;
using BlockFactory.Block_;
using BlockFactory.Client.Render.World_;
using BlockFactory.Item_;
using BlockFactory.Side_;
using BlockFactory.Util.Math_;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class HudRenderer : IDisposable
{
    public readonly BlockFactoryClient Client;

    public HudRenderer(BlockFactoryClient client, WorldRenderer renderer)
    {
        Client = client;
    }

    public void Render3DHud()
    {
        Client.Matrices.Push();
        double time = GLFW.GetTime();
        float sin = (float)Math.Sin(time);
        float cos = (float)Math.Cos(time);
        Client.Matrices.Translate(10 * Client.GetDirectionFromPosFor3DHud(
            new Vector2(0.8f + sin * 0.05f, 0.8f + cos * 0.05f)));
        // BlockMeshBuilder.MatrixStack.RotateX(MathF.PI / 6);
        Client.Matrices.RotateX(sin * 0.05f);
        Client.Matrices.RotateY(-MathF.PI / 12);
        Client.Matrices.Scale(4);
        Client.Matrices.Translate(new Vector3(-0.5f));
        ItemStack stack = Client.Player!.GetStackInHand();
        Client.ItemRenderer.RenderItemStack(stack);
        Client.Matrices.Pop();
    }

    public void Dispose()
    {
    }
}