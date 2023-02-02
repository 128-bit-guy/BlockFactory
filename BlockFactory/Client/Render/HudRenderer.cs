using BlockFactory.Client.Render.World_;
using BlockFactory.Side_;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class HudRenderer : IDisposable
{
    public readonly BlockFactoryClient Client;

    public HudRenderer(BlockFactoryClient client, WorldRenderer renderer)
    {
        Client = client;
    }

    public void Dispose()
    {
    }

    public void Render3DHud()
    {
        Client.Matrices.Push();
        var time = GLFW.GetTime();
        var sin = (float)Math.Sin(time);
        var cos = (float)Math.Cos(time);
        Client.Matrices.Translate(10 * Client.GetDirectionFromPosFor3DHud(
            new Vector2(0.8f + sin * 0.05f, 0.8f + cos * 0.05f)));
        // BlockMeshBuilder.MatrixStack.RotateX(MathF.PI / 6);
        Client.Matrices.RotateX(sin * 0.05f);
        Client.Matrices.RotateY(-MathF.PI / 12);
        Client.Matrices.Scale(4);
        Client.Matrices.Translate(new Vector3(-0.5f));
        var stack = Client.Player!.GetStackInHand();
        Client.ItemRenderer.RenderItemStack(stack);
        Client.Matrices.Pop();
    }
}