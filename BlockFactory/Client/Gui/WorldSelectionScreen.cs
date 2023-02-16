using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class WorldSelectionScreen : Screen
{
    public ButtonWidget Back;
    public ButtonWidget Delete;
    public ButtonWidget Create;
    public ButtonWidget Play;
    public WorldSelectionWidget WorldSelection;

    public WorldSelectionScreen(BlockFactoryClient client) : base(client)
    {
    }

    public override void InitWidgets(Vector2i size)
    {
        base.InitWidgets(size);
        var (width, height) = size;
        var centerX = width / 2f;
        var centerY = height / 2f;
        Widgets.Add(Back =
            new ButtonWidget(
                this,
                new Box2(centerX - 815, height - 80, centerX - 415, height - 10),
                "Back"));
        Back.OnClick += (_, _) => Client.PopScreen();
        Widgets.Add(Delete =
            new ButtonWidget(
                this,
                new Box2(centerX - 405, height - 80, centerX - 5, height - 10),
                "Delete"
                ));
        Delete.OnClick += (_, _) => OpenDeletion();
        Widgets.Add(Create =
            new ButtonWidget(
                this,
                new Box2(centerX + 5, height - 80, centerX + 405, height - 10),
                "Create"
                ));
        Create.OnClick += (_, _) => Client.PushScreen(new WorldCreationScreen(Client));
        Widgets.Add(Play =
            new ButtonWidget(
                this,
                new Box2(centerX + 415, height - 80, centerX + 815, height - 10),
                "Play"
                ));
        Play.OnClick += (_, _) => StartWorld();
        Widgets.Add(WorldSelection = new WorldSelectionWidget(
            this, 
            new Box2(10, 10, width - 10, height - 90)
            ));
    }

    private void StartWorld()
    {
        var s = WorldSelection.GetSelectedPath();
        if (s == null)
        {
            return;
        }
        Client.InitSingleplayerGameInstance(s);
        while (Client.HasScreen())
        {
            Client.PopScreen();
        }
    }

    private void OpenDeletion()
    {
        var selectedName = WorldSelection.GetSelectedPath();
        if (selectedName != null)
        {
            Client.PushScreen(new WorldDeletionScreen(Client, selectedName, WorldSelection.GetSelectedName()!));
        }
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        var worldSelected = WorldSelection.GetSelectedPath() != null;
        Play.Enabled = Delete.Enabled = worldSelected;
    }
}