using System.Reflection;
using System.Runtime.Loader;

namespace BlockFactory.Loader;

public class LoadingContext : AssemblyLoadContext
{
    private readonly string _loadingDirectory;

    public LoadingContext(string loadingDirectory)
    {
        _loadingDirectory = loadingDirectory;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        foreach (var assembly in GetLoadContext(typeof(LoadingContext).Assembly)!.Assemblies)
            if (assembly.GetName().Name == assemblyName.Name)
                return assembly;
        var dll = Path.Combine(_loadingDirectory, assemblyName.Name + ".dll");
        var pdb = Path.Combine(_loadingDirectory, assemblyName.Name + ".pdb");
        if (!File.Exists(dll)) return null;

        using var stream = new FileStream(dll, FileMode.Open);
        using var pdbStream = File.Exists(pdb) ? new FileStream(pdb, FileMode.Open) : null;
        return LoadFromStream(stream, pdbStream);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        foreach (var file in Directory.EnumerateFiles(_loadingDirectory, "*", SearchOption.AllDirectories))
            if (file.EndsWith(unmanagedDllName))
                return LoadUnmanagedDllFromPath(file);
        // Console.WriteLine(unmanagedDllName);
        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}