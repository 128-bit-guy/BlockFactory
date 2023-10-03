using Mono.Cecil;

namespace BlockFactory.DebugLauncher;

public class LauncherAssemblyResolver : DefaultAssemblyResolver
{
    private readonly string _loadingDirectory;

    public LauncherAssemblyResolver(string loadingDirectory)
    {
        _loadingDirectory = loadingDirectory;
    }

    public override AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        try
        {
            return base.Resolve(name);
        }
        catch (AssemblyResolutionException)
        {
            Console.WriteLine($"Resolving {name}");

            var dll = Path.Combine(_loadingDirectory, name.Name + ".dll");
            if (!File.Exists(dll))
            {
                Console.WriteLine($"Unable to resolve: {name}");
                throw;
            }

            return AssemblyDefinition.ReadAssembly(dll, new ReaderParameters());
        }
    }
}