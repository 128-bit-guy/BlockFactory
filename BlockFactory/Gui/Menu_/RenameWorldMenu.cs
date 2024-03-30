using BlockFactory.Client;
using BlockFactory.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Menu_;

public class RenameWorldMenu : Menu
{
    private readonly ButtonControl _cancelButton;
    private readonly ButtonControl _renameButton;
    private readonly SinglePlayerMenu _singlePlayerMenu;
    private readonly TextInputControl _textInput;
    private readonly string _worldName;

    public RenameWorldMenu(SinglePlayerMenu singlePlayerMenu, string worldName)
    {
        _singlePlayerMenu = singlePlayerMenu;
        _worldName = worldName;
        Root = new SlottedWindowControl(new Vector2D<int>(8, 4),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 7, 0, new LabelControl("Rename world"))
            .With(0, 1, 7, 1, _textInput = new TextInputControl())
            .With(0, 2, 7, 2, _renameButton = new ButtonControl("Rename"))
            .With(0, 3, 7, 3, _cancelButton = new ButtonControl("Cancel"));
        _textInput.Text = _worldName;
        _renameButton.Pressed += OnRenamePressed;
        _cancelButton.Pressed += OnCancelPressed;
        _textInput.EnterPressed += OnEnterPressed;
        _textInput.TextChanged += OnTextChanged;
        UpdateRenameEnabled();
    }

    private void OnTextChanged()
    {
        UpdateRenameEnabled();
    }

    private void UpdateRenameEnabled()
    {
        _renameButton.Enabled = _textInput.Text != "" &&
                                !Directory.Exists(Path.Combine(BlockFactoryClient.WorldsDirectory, _textInput.Text));
    }

    private void OnRenamePressed()
    {
        Apply();
    }

    private void OnEnterPressed()
    {
        if (_renameButton.Enabled) Apply();
    }

    private void Apply()
    {
        Directory.Move(Path.Combine(BlockFactoryClient.WorldsDirectory, _worldName),
            Path.Combine(BlockFactoryClient.WorldsDirectory, _textInput.Text));
        _singlePlayerMenu.UpdateWorldList();
        BlockFactoryClient.MenuManager.Pop();
    }

    private void OnCancelPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}