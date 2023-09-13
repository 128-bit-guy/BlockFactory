using Mono.Cecil;

namespace BlockFactory.Transformer;

public abstract class AssemblyTransformer<T> : IAssemblyTransformer
{
    public object ScanAssembly(AssemblyDefinition def)
    {
        return ScanAssemblyInternal(def)!;
    }

    public void TransformAssembly(AssemblyDefinition def, Func<string, object?> scanDataGetter)
    {
        TransformAssemblyInternal(def, s => (T?)scanDataGetter(s));
    }

    protected abstract T ScanAssemblyInternal(AssemblyDefinition definition);

    protected abstract void TransformAssemblyInternal(AssemblyDefinition def, Func<string, T?> scanDataGetter);
}