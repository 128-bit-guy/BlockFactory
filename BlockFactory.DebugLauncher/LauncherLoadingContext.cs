using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace BlockFactory.DebugLauncher;

public class LauncherLoadingContext : AssemblyLoadContext
{
    private readonly string _loadingDirectory;

    protected LauncherLoadingContext(string loadingDirectory)
    {
        _loadingDirectory = loadingDirectory;
    }

    protected LauncherLoadingContext(bool isCollectible, string loadingDirectory) : base(isCollectible)
    {
        _loadingDirectory = loadingDirectory;
    }

    public LauncherLoadingContext(string loadingDirectory, string? name, bool isCollectible = false) : base(name, isCollectible)
    {
        _loadingDirectory = loadingDirectory;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        try
        {
            var asm1 = Assembly.Load(assemblyName);
            Console.WriteLine($"Already loaded {assemblyName}");
            return asm1;
        }
        catch (FileNotFoundException e)
        {
        }

        var dll = Path.Combine(_loadingDirectory, assemblyName.Name + ".dll");
        var pdb = Path.Combine(_loadingDirectory, assemblyName.Name + ".pdb");
        if (!File.Exists(dll))
        {
            Console.WriteLine($"Not Found: {assemblyName}");
            return null;
        }

        using var stream = new FileStream(dll, FileMode.Open, FileAccess.Read);
        using var pdbStream = File.Exists(pdb) ? new FileStream(pdb, FileMode.Open, FileAccess.Read) : null;
        var asm = LoadFromStream(stream, pdbStream);
        Console.WriteLine($"Loaded {asm}");
        return asm;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var ext = ".so";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ext = ".dll";
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ext = ".dylib";
        }
        foreach (var file in Directory.EnumerateFiles(_loadingDirectory, "*", SearchOption.AllDirectories))
            if (file.EndsWith(unmanagedDllName + ext))
                return LoadUnmanagedDllFromPath(file);
        // Console.WriteLine(unmanagedDllName);
        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}