using BlockFactory.Base;
using BlockFactory.Content.Gui.Menu_;
using Silk.NET.Input;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public static class KeyboardInputManager
{
    public static void KeyDown(IKeyboard keyboard, Key key, int a)
    {
        if (key == Key.Escape)
        {
            if (BlockFactoryClient.MenuManager.Empty)
            {
                if (BlockFactoryClient.LogicProcessor != null) BlockFactoryClient.MenuManager.Push(new PauseMenu());
            }
            else
            {
                BlockFactoryClient.MenuManager.Top!.EscapePressed();
            }
        }
        else if (BlockFactoryClient.MenuManager.Empty)
        {
            if (BlockFactoryClient.Player != null)
            {
                if (key == Key.T)
                {
                    BlockFactoryClient.Player.HandleOpenMenuRequest(OpenMenuRequestType.Message);
                } else if (key == Key.E)
                {
                    BlockFactoryClient.Player.HandleOpenMenuRequest(OpenMenuRequestType.Inventory);
                }
            }
        }
        else
        {
            BlockFactoryClient.MenuManager.Top!.KeyDown(key, a);
        }
    }

    public static void KeyChar(IKeyboard keyboard, char c)
    {
        if (!BlockFactoryClient.MenuManager.Empty) BlockFactoryClient.MenuManager.Top!.KeyChar(c);
    }
}