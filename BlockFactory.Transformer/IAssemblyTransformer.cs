using Mono.Cecil;

namespace BlockFactory.Transformer;

public interface IAssemblyTransformer
{
    object ScanAssembly(AssemblyDefinition def);
    void TransformAssembly(AssemblyDefinition def, Func<string, object?> scanDataGetter);
}