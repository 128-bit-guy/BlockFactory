using BlockFactory.Client.Gui;
using Silk.NET.Input;

namespace BlockFactory.Client;

public static class KeyboardInputManager
{
    public static void KeyDown(IKeyboard keyboard, Key key, int a)
    {
        if (key == Key.Escape)
        {
            if (BlockFactoryClient.MenuManager.Empty)
            {
                if (BlockFactoryClient.LogicProcessor != null)
                {
                    BlockFactoryClient.MenuManager.Push(new PauseMenu());
                }
            }
            else
            {
                BlockFactoryClient.MenuManager.Top!.EscapePressed();
            }
        }
    }
}