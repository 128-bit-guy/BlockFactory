using BlockFactory.Gui;
using BlockFactory.Init;
using BlockFactory.Registry_;
using BlockFactory.Side_;

namespace BlockFactory.Client.Gui.InGame;

[ExclusiveTo(Side.Client)]
public class InGameMenuScreens
{
    public delegate InGameMenuScreen ScreenCreator(InGameMenu menu, BlockFactoryClient client);

    public static AttachmentRegistry<InGameMenuType, ScreenCreator> ScreenCreators;

    public static void Init()
    {
        ScreenCreators = new DefaultedAttachmentRegistry<InGameMenuType, ScreenCreator>(InGameMenuTypes.Registry,
            CreateDefault);
    }

    private static InGameMenuScreen CreateDefault(InGameMenu menu, BlockFactoryClient client)
    {
        return new InGameMenuScreen(menu, client);
    }
}