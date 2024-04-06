using BlockFactory.Entity_;
using BlockFactory.Gui.Control;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Menu_;

public class MessageMenu : SynchronizedMenu
{
    public MessageMenu(PlayerEntity user) : base(user)
    {
        Root = new SlottedWindowControl(new Vector2D<int>(5, 3),
                Array.Empty<int>(), Array.Empty<int>())
            .With(0, 0, 4, 0, new LabelControl("Send message"))
            .With(0, 1, 4, 1, new TextInputControl())
            .With(0, 2, 4, 2, new ButtonControl("Send"));
    }
}