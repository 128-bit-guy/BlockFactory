using BlockFactory.Registry_;

namespace BlockFactory.Gui.Widget;

public class InGameMenuWidgetType : IRegistryEntry
{
    public delegate InGameMenuWidget WidgetLoader(BinaryReader reader);

    public delegate InGameMenuWidget WidgetLoaderWithType(InGameMenuWidgetType type, BinaryReader reader);

    public readonly WidgetLoader Loader;

    public InGameMenuWidgetType(WidgetLoaderWithType loader)
    {
        Loader = reader => loader(this, reader);
    }

    public int Id { get; set; }
}