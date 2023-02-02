using BlockFactory.Registry_;
using BlockFactory.Util;

namespace BlockFactory.Gui;

public class InGameMenuType : IRegistryEntry
{
    public delegate InGameMenu MenuLoader(BinaryReader reader);

    public delegate InGameMenu MenuLoaderWithType(InGameMenuType type, BinaryReader reader);

    public readonly MenuLoader Loader;

    public InGameMenuType(MenuLoaderWithType loader)
    {
        Loader = reader => loader(this, reader);
    }

    public InGameMenuType(Type t) : this(GetMenuLoaderWithType(t))
    {
    }

    public int Id { get; set; }

    private static MenuLoaderWithType GetMenuLoaderWithType(Type t)
    {
        return t.GetConstructor(new[] { typeof(InGameMenuType), typeof(BinaryReader) })!
            .CreateDelegate<MenuLoaderWithType>();
    }
}