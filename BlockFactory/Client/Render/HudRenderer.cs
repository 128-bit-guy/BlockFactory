using BlockFactory.Client.Gui;
using BlockFactory.Client.Render.Shader;
using BlockFactory.Client.Render.World_;
using BlockFactory.Entity_.Player;
using BlockFactory.Item_;
using BlockFactory.Side_;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class HudRenderer : IDisposable
{
    public readonly BlockFactoryClient Client;
    private Screen _fakeRenderScreen;

    public HudRenderer(BlockFactoryClient client, WorldRenderer renderer)
    {
        Client = client;
        _fakeRenderScreen = new Screen(client);
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
        var stack = Client.Player!.StackInHand;
        Client.ItemRenderer!.RenderItemStack(stack);
        Client.Matrices.Pop();
    }

    private void RenderHP(Box2 box)
    {
        var hp = Client.Player!.Health;
        var ratio = hp / (float)Client.Player!.MaxHealth;
        var midX = MathHelper.Lerp(box.Min.X, box.Max.X, ratio);
        var midT = 10 * ratio;
        {
            _fakeRenderScreen.GuiMesh.Builder.MatrixStack.Push();
            _fakeRenderScreen.GuiMesh.Builder.BeginIndexSpace();
            _fakeRenderScreen.GuiMesh.Builder.AddIndices(0, 2, 1, 0, 3, 2);
            var miX = box.Min.X;
            var maX = midX;
            var miY = box.Min.Y;
            var maY = box.Max.Y;
            var miTX = 0;
            var maTX = midT;
            var miTY = 1;
            var maTY = 0;
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((miX, miY, 1, 1, 1, 1, miTX, miTY));
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((maX, miY, 1, 1, 1, 1, maTX, miTY));
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((maX, maY, 1, 1, 1, 1, maTX, maTY));
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((miX, maY, 1, 1, 1, 1, miTX, maTY));
            _fakeRenderScreen.GuiMesh.Builder.EndIndexSpace();
            _fakeRenderScreen.GuiMesh.Builder.MatrixStack.Pop();
            Shaders.Gui.Use();
            Client.VpMatrices.Set(Shaders.Gui);
            Shaders.Gui.SetModel(Client.Matrices);
            Textures.HeartTexture.BindTexture();
            _fakeRenderScreen.GuiMesh.Flush();
        }
        {
            _fakeRenderScreen.GuiMesh.Builder.MatrixStack.Push();
            _fakeRenderScreen.GuiMesh.Builder.BeginIndexSpace();
            _fakeRenderScreen.GuiMesh.Builder.AddIndices(0, 2, 1, 0, 3, 2);
            var miX = midX;
            var maX = box.Max.X;
            var miY = box.Min.Y;
            var maY = box.Max.Y;
            var miTX = midT;
            var maTX = 10;
            var miTY = 1;
            var maTY = 0;
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((miX, miY, 1, 1, 1, 1, miTX, miTY));
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((maX, miY, 1, 1, 1, 1, maTX, miTY));
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((maX, maY, 1, 1, 1, 1, maTX, maTY));
            _fakeRenderScreen.GuiMesh.Builder.AddVertex((miX, maY, 1, 1, 1, 1, miTX, maTY));
            _fakeRenderScreen.GuiMesh.Builder.EndIndexSpace();
            _fakeRenderScreen.GuiMesh.Builder.MatrixStack.Pop();
            Shaders.Gui.Use();
            Client.VpMatrices.Set(Shaders.Gui);
            Shaders.Gui.SetModel(Client.Matrices);
            Textures.EmptyHeartTexture.BindTexture();
            _fakeRenderScreen.GuiMesh.Flush();
        }
    }

    public void Render2DHud()
    {
        var (width, height) = Client.GetDimensions();
        var center = new Vector2(width, height) / 2;
        _fakeRenderScreen.DrawStretchedTexturedRect(new Box2(center - new Vector2(32), center + new Vector2(32)), 1, 1,
            Textures.CrosshairTexture);
        var hotbarBox = new Box2(center.X - 384, height - 96, center.X + 384, height);
        _fakeRenderScreen.DrawStretchedTexturedRect(hotbarBox, 1, 1, Textures.HotbarTexture);
        PlayerEntity player = Client.Player!;
        var slotBeginY = hotbarBox.Min.Y;
        var slotEndY = hotbarBox.Max.Y;
        for (var i = 0; i < 9; ++i)
        {
            var slotBegin = MathHelper.Lerp(hotbarBox.Min.X, hotbarBox.Max.X, (28 * i) / 256f);
            var slotEnd = slotBegin + (hotbarBox.Max.X - hotbarBox.Min.X) * (32 / 256f);
            var slotBox = new Box2(slotBegin, slotBeginY, slotEnd, slotEndY);
            if (player.HotbarPos == i)
            {
                _fakeRenderScreen.DrawStretchedTexturedRect(slotBox, 2, 1, Textures.HotbarSelectedTexture);
            }

            var stack = player.Hotbar[i];
            var slotCenter = slotBox.Center;
            Client.Matrices.Push();
            Client.Matrices.Translate((slotCenter.X, slotCenter.Y, 3));
            _fakeRenderScreen.DrawStack(stack);
            Client.Matrices.Pop();
        }

        var hpBox = new Box2(hotbarBox.Min.X, hotbarBox.Min.Y - 32, hotbarBox.Min.X + 320,
            hotbarBox.Min.Y);
        RenderHP(hpBox);
    }
}