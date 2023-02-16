using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class WorldCreationScreen : Screen
{
    public ButtonWidget BackButton = null!;
    public ButtonWidget CreateButton = null!;
    public TextInputWidget TextInput = null!;

    public WorldCreationScreen(BlockFactoryClient client) : base(client)
    {
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
        Widgets.Add(CreateButton = new ButtonWidget(this,
            new Box2(centerX - 300, centerY + 110, centerX + 300, centerY + 180), "Create"));
        CreateButton.OnClick += (_, _) => Create();
        Widgets.Add(TextInput =
            new TextInputWidget(this, new Box2(centerX - 300, centerY + 30, centerX + 300, centerY + 100)));
        TextInput.OnEnterPressed += Create;
        TextInput.OnTextChanged += ValidateWorldName;
        ValidateWorldName();
    }

    private void Create()
    {
        var s = GetWorldPath();
        Client.InitSingleplayerGameInstance(s);
        while (Client.HasScreen())
        {
            Client.PopScreen();
        }
    }

    private string GetWorldPath()
    {
        return Path.Combine(Client.WorldsDirectory, TextInput.Text);
    }

    private void ValidateWorldName()
    {
        if (TextInput.Text == string.Empty)
        {
            CreateButton.Enabled = false;
            return;
        }

        if (Directory.Exists(GetWorldPath()))
        {
            CreateButton.Enabled = false;
            return;
        }

        CreateButton.Enabled = true;
    }

    public override void UpdateAndRender()
    {
        base.UpdateAndRender();
        var (width, height) = Client.GetDimensions();
        Client.Matrices.Push();
        Client.Matrices.Translate((width / 2f, height / 2 - 240, 1));
        DrawText("Create world", 0);
        Client.Matrices.Pop();
    }
}