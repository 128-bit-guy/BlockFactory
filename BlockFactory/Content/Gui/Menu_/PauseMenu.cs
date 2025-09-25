using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

[ExclusiveTo(Side.Client)]
public class PauseMenu : Menu
{
    private readonly ButtonControl _backToGame;
    private readonly ButtonControl _saveAndQuit;

    public PauseMenu()
    {
        Root = new SlottedWindowControl(new Vector2D<int>(5, 5),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 4, 0, new LabelControl("Pause Menu"))
            .With(0, 1, 4, 1, _saveAndQuit = new ButtonControl("Save And Quit"))
            .With(0, 4, 4, 4, _backToGame = new ButtonControl("Back To Game"));
        _saveAndQuit.Pressed += SaveAndQuitPressed;
        _backToGame.Pressed += BackToGamePressed;
    }

    private void SaveAndQuitPressed()
    {
        BlockFactoryClient.ExitWorld();
        // BlockFactoryClient.MenuManager.Pop();
    }

    private void BackToGamePressed()
    {
        BlockFactoryClient.MenuManager.Pop();
    }
}