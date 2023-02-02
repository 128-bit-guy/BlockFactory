using BlockFactory.Client.Render;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class ForcedDisconnectionScreen : Screen
{
    public readonly string Reason;
    public ButtonWidget BackButton = null!;

    public ForcedDisconnectionScreen(BlockFactoryClient client, string reason) : base(client)
    {
        var cnt = 0;
        for (var i = 0; i < reason.Length; ++i)
        {
            if (reason[i] == '\n') ++cnt;
            if (cnt == 5)
            {
                Reason = reason.Substring(0, i) + "\n...";
                return;
            }
        }

        Reason = reason;
    }

    public ForcedDisconnectionScreen(BlockFactoryClient client, Exception exception) : this(client,
        exception.GetType().Name + ": " + exception.Message + "\n" + exception.StackTrace)
    {
    }

    public override void InitWidgets(Vector2i size)
    {
        base.InitWidgets(size);
        var (width, height) = size;
        var stringHeight = Textures.TextRenderer.GetStringHeight(Reason);
        var guiHeight = stringHeight + 80;
        var centerX = width / 2f;
        var centerY = height / 2f;
        var guiTop = centerY - guiHeight / 2;
        Widgets.Add(BackButton = new ButtonWidget(this,
            new Box2(centerX - 300, guiTop + stringHeight + 10, centerX + 300, guiTop + stringHeight + 80), "Back"));
        BackButton.OnClick += (_, _) => Client.PopScreen();
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        var (width, height) = Client.GetDimensions();
        var stringHeight = Textures.TextRenderer.GetStringHeight(Reason);
        var guiHeight = stringHeight + 80;
        var centerX = width / 2f;
        var centerY = height / 2f;
        var guiTop = centerY - guiHeight / 2;
        Client.Matrices.Push();
        Client.Matrices.Translate((width / 2f, guiTop, 1));
        DrawText(Reason, 0);
        Client.Matrices.Pop();
    }
}