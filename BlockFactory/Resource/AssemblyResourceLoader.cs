using System.Reflection;

namespace BlockFactory.Resource;

public class AssemblyResourceLoader : ResourceLoader
{
    private readonly Assembly _assembly;

    public AssemblyResourceLoader(Assembly assembly)
    {
        _assembly = assembly;
    }

    public override Stream? GetResourceStream(string s)
    {
        return _assembly.GetManifestResourceStream(s);
    }
}