using System.Reflection;
using BlockFactory.Side_;

namespace BlockFactory.Resource;

[ExclusiveTo(Side.Client)]
public class AssemblyResourceLoader : IResourceLoader
{
    private readonly Assembly _assembly;

    public AssemblyResourceLoader(Assembly assembly)
    {
        _assembly = assembly;
    }

    public Stream? GetResourceStream(string location)
    {
        return _assembly.GetManifestResourceStream(location);
    }
}