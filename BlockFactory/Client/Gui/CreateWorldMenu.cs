using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class CreateWorldMenu : Menu
{
    private readonly TextInputControl _textInput;
    private readonly ButtonControl _createButton;
    private readonly ButtonControl _cancelButton;

    public CreateWorldMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(8, 4),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 7, 0, new LabelControl("Create world"))
            .With(0, 1, 7, 1, _textInput = new TextInputControl())
            .With(0, 2, 7, 2, _createButton = new ButtonControl("Create"))
            .With(0, 3, 7, 3, _cancelButton = new ButtonControl("Cancel"));
        _createButton.Pressed += OnCreatePressed;
        _cancelButton.Pressed += OnCancelPressed;
        _textInput.Text = GetInitialWorldName();
        _textInput.EnterPressed += OnEnterPressed;
        _textInput.TextChanged += OnTextChanged;
        UpdateRenameEnabled();
    }

    private static string GetInitialWorldName()
    {
        for (var i = 1;; ++i)
        {
            var name = $"BlockFactory World {i}";
            if (!Directory.Exists(Path.Combine(BlockFactoryClient.WorldsDirectory, name)))
            {
                return name;
            }
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
        if (_createButton.Enabled)
        {
            Apply();
        }
    }

    private void Apply()
    {
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.StartSinglePlayer(
            Path.Combine(BlockFactoryClient.WorldsDirectory, _textInput.Text)
            );
    }

    private void OnCancelPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}