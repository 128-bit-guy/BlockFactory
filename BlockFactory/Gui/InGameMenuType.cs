using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Registry_;
using BlockFactory.Util;

namespace BlockFactory.Gui;

public class InGameMenuType : IRegistryEntry
{
    public int Id { get; set; }

    public delegate InGameMenu MenuLoaderWithType(InGameMenuType type, BinaryReader reader);

    public delegate InGameMenu MenuLoader(BinaryReader reader);

    public readonly MenuLoader Loader;

    public InGameMenuType(MenuLoaderWithType loader)
    {
        Loader = reader => loader(this, reader);
    }

    private static MenuLoaderWithType GetMenuLoaderWithType(Type t)
    {
        return t.GetConstructor(new[] { typeof(InGameMenuType), typeof(BinaryReader) })!
            .CreateDelegate<MenuLoaderWithType>();
    }

    public InGameMenuType(Type t) : this(GetMenuLoaderWithType(t))
    {
        
    }
}