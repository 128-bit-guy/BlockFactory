using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class DeleteWorldMenu : Menu
{
    private readonly ButtonControl _noButton;
    private readonly SinglePlayerMenu _singlePlayerMenu;
    private readonly string _worldName;
    private readonly ButtonControl _yesButton;

    public DeleteWorldMenu(SinglePlayerMenu singlePlayerMenu, string worldName)
    {
        _singlePlayerMenu = singlePlayerMenu;
        _worldName = worldName;
        Root = new SlottedWindowControl(new Vector2D<int>(10, 5),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 9, 0,
                new LabelControl("Are you sure?"))
            .With(0, 1, 9, 1,
                new LabelControl(worldName))
            .With(0, 2, 9, 2,
                new LabelControl("Will be deleted irreversibly."))
            .With(2, 3, 7, 3, _yesButton = new ButtonControl("Yes"))
            .With(2, 4, 7, 4, _noButton = new ButtonControl("No"));
        _yesButton.Pressed += OnYesPressed;
        _noButton.Pressed += OnNoPressed;
    }

    private void OnYesPressed()
    {
        Directory.Delete(Path.Combine(BlockFactoryClient.WorldsDirectory, _worldName), true);
        _singlePlayerMenu.UpdateWorldList();
        BlockFactoryClient.MenuManager.Pop();
    }

    private void OnNoPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}