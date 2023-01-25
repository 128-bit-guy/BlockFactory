using System.Reflection;
using BlockFactory.Side_;
using BlockFactory.SideStripper_;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace BlockFactory.Loader;

public class BlockFactoryLoader
{
    private readonly Side _side;
    private Assembly _assembly = null!;
    private LoadingContext _context = null!;

    public BlockFactoryLoader(Side side)
    {
        _side = side;
    }

    private void CreateContext(string targetPath)
    {
        _context = new LoadingContext(targetPath);
    }

    private void ModifyAssembly(string targetPath, string blockFactoryDll, Stream stream, Stream pdbStream)
    {
        var assemblyPath = Path.GetFullPath(Path.Combine(targetPath, blockFactoryDll));
        var readerParameters = new ReaderParameters
        {
            ReadSymbols = true
        };
        var def = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
        var stripper = new SideStripper(_side, new[] { def });
        stripper.Process();
        var writerParameters = new WriterParameters
        {
            WriteSymbols = true,
            SymbolStream = pdbStream,
            SymbolWriterProvider = new PortablePdbWriterProvider()
        };
        stripper.Definitions[0].Write(stream, writerParameters);
    }

    private void CreateAssembly(Stream stream, Stream pdbStream)
    {
        _assembly = _context.LoadFromStream(stream, pdbStream);
    }

    private void RunBlockFactory(string[] args)
    {
        foreach (var method in _assembly.DefinedTypes
                     .SelectMany(t => t.DeclaredMethods)
                     .Where(m => m.IsStatic))
            if (method.GetCustomAttributes().Where(t => t is SidedEntryPointAttribute)
                .Any(t => ((SidedEntryPointAttribute)t).Side == _side))
            {
                method.Invoke(null, new object?[] { args });
                return;
            }

        throw new ArgumentException($"Could not find any sided entry point for side {_side} in block factory assembly");
    }

    public void Load(string targetPath, string blockFactoryDll, string[] args)
    {
        CreateContext(targetPath);
        using var stream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        ModifyAssembly(targetPath, blockFactoryDll, stream, pdbStream);
        stream.Seek(0, SeekOrigin.Begin);
        pdbStream.Seek(0, SeekOrigin.Begin);
        CreateAssembly(stream, pdbStream);
        RunBlockFactory(args);
    }

    public void LoadUnstriped(Assembly assembly, string[] args)
    {
        _assembly = assembly;
        RunBlockFactory(args);
    }
}