using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace BlockFactory.Transformer;

public class TransformerManager
{
    private readonly AssemblyDefinition[] _definitions;
    private readonly IAssemblyTransformer[] _transformers;

    public TransformerManager(AssemblyDefinition[] definitions, IAssemblyTransformer[] transformers)
    {
        _definitions = definitions;
        _transformers = transformers;
    }

    private static AssemblyDefinition LoadAssemblyDefinition(string path)
    {
        var readerParameters = new ReaderParameters
        {
            ReadSymbols = true
        };
        return AssemblyDefinition.ReadAssembly(path, readerParameters);
    }

    public TransformerManager(string[] assemblyPaths, IAssemblyTransformer[] transformers) : this(
        assemblyPaths.Select(LoadAssemblyDefinition).ToArray(), transformers)
    {
    }

    public void Process()
    {
        var assembliesByNames = new Dictionary<string, int>();
        for (var i = 0; i < _definitions.Length; ++i)
        {
            assembliesByNames[_definitions[i].FullName] = i;
        }
        foreach (var transformer in _transformers)
        {
            var scanDatas = _definitions.Select(transformer.ScanAssembly).ToArray();
            Func<string, object> scanDataGetter = s => scanDatas[assembliesByNames[s]];
            foreach (var assembly in _definitions)
            {
                transformer.TransformAssembly(assembly, scanDataGetter);
            }
        }
    }

    public void WriteAssembly(int i, Stream asmStream, Stream pdbStream)
    {
        var writerParameters = new WriterParameters
        {
            WriteSymbols = true,
            SymbolStream = pdbStream,
            SymbolWriterProvider = new PortablePdbWriterProvider()
        };
        _definitions[i].Write(asmStream, writerParameters);
    }

    public Assembly[] LoadAssemblies(AssemblyLoadContext context)
    {
        var res = new Assembly[_definitions.Length];
        for (var i = 0; i < res.Length; ++i)
        {
            using var stream = new MemoryStream();
            using var pdbStream = new MemoryStream();
            WriteAssembly(i, stream, pdbStream);
            stream.Seek(0, SeekOrigin.Begin);
            pdbStream.Seek(0, SeekOrigin.Begin);
            res[i] = context.LoadFromStream(stream, pdbStream);
        }

        return res;
    }
}