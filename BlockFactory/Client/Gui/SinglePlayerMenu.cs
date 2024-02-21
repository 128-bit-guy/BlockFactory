using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class SinglePlayerMenu : Menu
{
    private readonly ScrollBarControl _scrollBar;
    private readonly ButtonControl _playButton;
    private readonly ButtonControl _renameButton;
    private readonly ButtonControl _createButton;
    private readonly ButtonControl _deleteButton;
    private readonly ButtonControl _backButton;
    private readonly ButtonControl[] _worldButtons;
    private int[] _worldButtonIndices;
    private List<string> _worldList = new();
    private int _currentSelected = -1;

    public SinglePlayerMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(8, 11),
                Array.Empty<int>(), new[] { 7, 9 })
            .With(0, 0, 7, 0, new LabelControl("Singleplayer"))
            .With(7, 1, 7, 7, _scrollBar = new ScrollBarControl())
            .With(0, 8, 3, 8, _playButton = new ButtonControl("Play"))
            .With(4, 8, 7, 8, _renameButton = new ButtonControl("Rename"))
            .With(0, 9, 3, 9, _createButton = new ButtonControl("Create"))
            .With(4, 9, 7, 9, _deleteButton = new ButtonControl("Delete"))
            .With(0, 10, 7, 10, _backButton = new ButtonControl("Back"));
        _worldButtons = new ButtonControl[7];
        _worldButtonIndices = new int[_worldButtons.Length];
        for (var i = 0; i < _worldButtons.Length; ++i)
        {
            ((SlottedWindowControl)Root).With(0, 1 + i, 6, 1 + i,
                _worldButtons[i] = new ButtonControl($"World #{i + 1}"));
            var j = i;
            _worldButtons[i].Pressed += () =>
            {
                _currentSelected = _worldButtonIndices[j];
                UpdateButtons();
            };
        }

        _backButton.Pressed += OnBackPressed;
        _deleteButton.Pressed += OnDeletePressed;
        _renameButton.Pressed += OnRenamePressed;
        _playButton.Pressed += OnPlayPressed;
        _createButton.Pressed += OnCreatePressed;
        _scrollBar.PositionChanged += UpdateButtons;
        UpdateWorldList();
    }

    public void UpdateWorldList()
    {
        _worldList.Clear();
        foreach (var dir in Directory.GetDirectories(BlockFactoryClient.WorldsDirectory))
        {
            _worldList.Add(Path.GetRelativePath(BlockFactoryClient.WorldsDirectory, dir));
        }

        _scrollBar.Positions = Math.Max(_worldList.Count - _worldButtons.Length + 1, 1);

        _scrollBar.CurrentPosition = 0;

        _currentSelected = -1;

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        for (var i = 0; i < _worldButtons.Length; ++i)
        {
            _worldButtonIndices[i] = _scrollBar.CurrentPosition + i;
            var cWorld = _worldButtonIndices[i] < _worldList.Count ? _worldList[_worldButtonIndices[i]] : null;
            _worldButtons[i].Enabled = cWorld != null && _worldButtonIndices[i] != _currentSelected;
            _worldButtons[i].Text = cWorld ?? "-----";
        }

        var somethingSelected = _currentSelected != -1;
        _playButton.Enabled = _renameButton.Enabled = _deleteButton.Enabled = somethingSelected;
    }

    private void OnBackPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }

    private void OnDeletePressed()
    {
        BlockFactoryClient.MenuManager.Push(new DeleteWorldMenu(this, _worldList[_currentSelected]));
    }

    private void OnRenamePressed()
    {
        BlockFactoryClient.MenuManager.Push(new RenameWorldMenu(this, _worldList[_currentSelected]));
    }

    private void OnPlayPressed()
    {
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.StartSinglePlayer(Path.Combine(BlockFactoryClient.WorldsDirectory,
            _worldList[_currentSelected]));
    }

    private void OnCreatePressed()
    {
        BlockFactoryClient.MenuManager.Push(new CreateWorldMenu());
    }
}