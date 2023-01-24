using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace BlockFactory.SideStripper_;

public static class IlUtils
{
    public static bool IsCompilerGenerated(IEnumerable<CustomAttribute> attributes)
    {
        return attributes
            .Any(attr => attr.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
    }

    public static FieldReference? GetBackingField(this PropertyDefinition def)
    {
        if (def.GetMethod == null)
        {
            return null;
        }

        var get = def.GetMethod;
        if (get.IsAbstract)
        {
            return null;
        }

        if (!IsCompilerGenerated(get.CustomAttributes))
        {
            return null;
        }
        
        var field = get.Body.Instructions[1].Operand;
        if (field is FieldReference f)
        {
            return f;
        }

        return null;
    }

    public static void RemoveIf<T>(this Collection<T> collection, Predicate<T> predicate)
    {
        var l = collection.Where(t => predicate(t)).ToList();
        foreach (var t in l)
        {
            collection.Remove(t);
        }
    }
}