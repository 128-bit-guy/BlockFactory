using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class WorldDeletionScreen : Screen
{
    public ButtonWidget BackButton = null!;
    public ButtonWidget DeleteButton = null!;
    public readonly string WorldPath;
    public readonly string WorldName;
    
    public WorldDeletionScreen(BlockFactoryClient client, string worldPath, string worldName) : base(client)
    {
        WorldPath = worldPath;
        WorldName = worldName;
    }

    public override void InitWidgets(Vector2i size)
    {
        base.InitWidgets(size);
        var (width, height) = size;
        var centerX = width / 2f;
        var centerY = height / 2f;
        Widgets.Add(BackButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY + 190, centerX + 300, centerY + 260), "Back"));
        BackButton.OnClick += (_, _) => Client.PopScreen();
        Widgets.Add(DeleteButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY + 110, centerX + 300, centerY + 180), "Delete"));
        DeleteButton.OnClick += (_, _) => Delete();
    }

    private void Delete()
    {
        Directory.Delete(WorldPath, true);
        Client.PopScreen();
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        var (width, height) = Client.GetDimensions();
        Client.Matrices.Push();
        Client.Matrices.Translate((width / 2f, height / 2 - 240, 1));
        DrawText($"Are you sure you want to delete world \"{WorldName}\"?", 0);
        Client.Matrices.Pop();
    }
}