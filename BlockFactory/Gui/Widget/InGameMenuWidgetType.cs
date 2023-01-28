using BlockFactory.Registry_;

namespace BlockFactory.Gui.Widget;

public class InGameMenuWidgetType : IRegistryEntry
{
    public int Id { get; set; }

    public delegate InGameMenuWidget WidgetLoaderWithType(InGameMenuWidgetType type, BinaryReader reader);
    
    public delegate InGameMenuWidget WidgetLoader(BinaryReader reader);

    public readonly WidgetLoader Loader;
    
    public InGameMenuWidgetType(WidgetLoaderWithType loader)
    {
        Loader = reader => loader(this, reader);
    }
}