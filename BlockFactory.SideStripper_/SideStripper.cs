using System.Collections.ObjectModel;
using BlockFactory.Side_;
using Mono.Cecil;

namespace BlockFactory.SideStripper_;

public class SideStripper
{
    private readonly Side _side;
    public readonly AssemblyDefinition[] Definitions;
    private readonly HashSet<string> _excludedTypes;
    private readonly HashSet<string> _excludedMethods;
    private readonly HashSet<string> _excludedFields;
    private readonly HashSet<string> _excludedProperties;

    public SideStripper(Side side, AssemblyDefinition[] definitions)
    {
        _side = side;
        Definitions = definitions;
        _excludedTypes = new HashSet<string>();
        _excludedMethods = new HashSet<string>();
        _excludedFields = new HashSet<string>();
        _excludedProperties = new HashSet<string>();
    }

    private bool IsExcluded(IEnumerable<CustomAttribute> attributes)
    {
        return attributes
            .Where(attr => attr.AttributeType.FullName == typeof(ExclusiveToAttribute).FullName)
            .Any(attr => ((Side)attr.ConstructorArguments[0].Value) != _side);
    }

    private void FindExcludedThings()
    {
        foreach (
            var type in Definitions
                .SelectMany(a => a.Modules)
                .SelectMany(m => m.Types)
        )
        {
            if (IsExcluded(type.CustomAttributes))
            {
                Console.WriteLine($"Excluding type {type.FullName}");
                _excludedTypes.Add(type.FullName);
                continue;
            }
            foreach (var method in type.Methods.Where(method => IsExcluded(method.CustomAttributes)))
            {
                Console.WriteLine($"Excluding method {method.FullName}");
                _excludedMethods.Add(method.FullName);
            }
            foreach (var field in type.Fields.Where(field => IsExcluded(field.CustomAttributes)))
            {
                Console.WriteLine($"Excluding field {field.FullName}");
                _excludedFields.Add(field.FullName);
            }

            foreach (var property in type.Properties.Where(property => IsExcluded(property.CustomAttributes)))
            {
                Console.WriteLine($"Excluding property {property.FullName} and it's methods");
                _excludedProperties.Add(property.FullName);
                if (property.GetMethod != null)
                {
                    _excludedMethods.Add(property.GetMethod.FullName);
                }

                if (property.SetMethod != null)
                {
                    _excludedMethods.Add(property.SetMethod.FullName);
                }

                var field = property.GetBackingField();
                if (field != null)
                {
                    _excludedFields.Add(field.FullName);
                }
            }
        }
    }

    private void RemoveExcludedThings()
    {
        foreach (var module in Definitions.SelectMany(def => def.Modules))
        {
            module.Types.RemoveIf(t => _excludedTypes.Contains(t.FullName));
            foreach (var t in module.Types)
            {
                t.Fields.RemoveIf(f => _excludedFields.Contains(f.FullName));
                t.Methods.RemoveIf(m => _excludedMethods.Contains(m.FullName));
                t.Properties.RemoveIf(p => _excludedProperties.Contains(p.FullName));
            }
        }
    }

    private void FixDependentCode()
    {
    }

    public void Process()
    {
        FindExcludedThings();
        RemoveExcludedThings();
        FixDependentCode();
    }
}