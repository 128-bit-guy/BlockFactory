using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Gui.Control;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

[ExclusiveTo(Side.Client)]
public class CreateWorldMenu : Menu
{
    private readonly ButtonControl _cancelButton;
    private readonly ButtonControl _createButton;
    private readonly WorldSettings _settings;
    private readonly TextInputControl _textInput;
    private readonly ButtonControl _typeButton;

    public CreateWorldMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(8, 5),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 7, 0, new LabelControl("Create world"))
            .With(0, 1, 7, 1, _textInput = new TextInputControl())
            .With(0, 2, 7, 2, _typeButton = new ButtonControl("World Type"))
            .With(0, 3, 7, 3, _createButton = new ButtonControl("Create"))
            .With(0, 4, 7, 4, _cancelButton = new ButtonControl("Cancel"));
        _createButton.Pressed += OnCreatePressed;
        _cancelButton.Pressed += OnCancelPressed;
        _textInput.Text = GetInitialWorldName();
        _textInput.EnterPressed += OnEnterPressed;
        _textInput.TextChanged += OnTextChanged;
        _typeButton.Pressed += OnTypePressed;
        _settings = new WorldSettings(DateTime.UtcNow.Ticks, false);
        UpdateTypeButtonText();
        UpdateRenameEnabled();
    }

    private void OnTypePressed()
    {
        _settings.Flat = !_settings.Flat;
        UpdateTypeButtonText();
    }

    private void UpdateTypeButtonText()
    {
        var type = _settings.Flat ? "Flat" : "Terrain";
        _typeButton.Text = $"World type: {type}";
    }

    private static string GetInitialWorldName()
    {
        for (var i = 1;; ++i)
        {
            var name = $"BlockFactory World {i}";
            if (!Directory.Exists(Path.Combine(BlockFactoryClient.WorldsDirectory, name))) return name;
        }
    }

    private void OnTextChanged()
    {
        UpdateRenameEnabled();
    }

    private void UpdateRenameEnabled()
    {
        _createButton.Enabled = _textInput.Text != "" &&
                                !Directory.Exists(Path.Combine(BlockFactoryClient.WorldsDirectory, _textInput.Text));
    }

    private void OnCreatePressed()
    {
        Apply();
    }

    private void OnEnterPressed()
    {
        if (_createButton.Enabled) Apply();
    }

    private void Apply()
    {
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.StartSinglePlayer(
            Path.Combine(BlockFactoryClient.WorldsDirectory, _textInput.Text), _settings
        );
    }

    private void OnCancelPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}