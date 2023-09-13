using Mono.Cecil;

namespace BlockFactory.Transformer;

public static class TypeEnumerators
{
    public static IEnumerable<TypeDefinition> AllTypes(this TypeDefinition definition)
    {
        yield return definition;
        foreach (var nested2 in definition.NestedTypes.SelectMany(nested => nested.AllTypes()))
        {
            yield return nested2;
        }
    }
    public static IEnumerable<TypeDefinition> AllTypes(this ModuleDefinition definition)
    {
        return definition.Types.SelectMany(AllTypes);
    }

    public static IEnumerable<TypeDefinition> AllTypes(this AssemblyDefinition definition)
    {
        return definition.Modules.SelectMany(AllTypes);
    }
}