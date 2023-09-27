using System.Reflection;
using BlockFactory.Base;
using BlockFactory.Transformer;
using BlockFactory.Transformer.SideStripper;
using Silk.NET.Core.Loader;

namespace BlockFactory.DebugLauncher;

public static class DebugLauncher
{
    public static void Launch(Side side)
    {
        Console.WriteLine($"Launching BlockFactory {side}");
        var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var projPath = Path.GetFullPath(Path.Combine(buildPath, "..", "..", ".."));
        var solPath = Path.GetFullPath(Path.Combine(projPath, ".."));
        var bfProjPath = Path.GetFullPath(Path.Combine(solPath, "BlockFactory.UnmodifiedClient"));
        var buildDeltaPath = Path.GetRelativePath(projPath, buildPath);
        var bfBuildPath = Path.GetFullPath(Path.Combine(bfProjPath, buildDeltaPath));
        var buildNativePath = Path.GetFullPath(Path.Combine(buildPath, "runtimes"));
        var bfNativePath = Path.GetFullPath(Path.Combine(bfBuildPath, "runtimes"));
        if (Directory.Exists(buildNativePath))
        {
            Directory.Delete(buildNativePath, true);
        }

        foreach (var name in Directory.EnumerateFiles(bfNativePath, "*", SearchOption.AllDirectories))
        {
            var deltaPath = Path.GetRelativePath(bfNativePath, name);
            var newPath = Path.GetFullPath(Path.Combine(buildNativePath, deltaPath));
            var newDir = Path.GetDirectoryName(newPath)!;
            Directory.CreateDirectory(newDir);
            File.Copy(name, newPath);
        }

        var bfAssemblyPath = Path.GetFullPath(Path.Combine(bfBuildPath, "BlockFactory.dll"));
        var context = new LauncherLoadingContext(bfBuildPath, "Block Factory debug context");
        var manager = new TransformerManager(new[] { bfAssemblyPath },
            new LauncherAssemblyResolver(bfBuildPath),
            new IAssemblyTransformer[] { new SideStripperTransformer(side) });
        manager.Process();
        var assemblies = manager.LoadAssemblies(context);
        var entryPoint = assemblies[0].DefinedTypes.SelectMany(t => t.DeclaredMethods).First(m =>
            m.GetCustomAttributes().Any(a => a is EntryPointAttribute epa && epa.Side == side));
        entryPoint.Invoke(null, Array.Empty<object>());
    }
}