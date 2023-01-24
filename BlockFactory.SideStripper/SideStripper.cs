using System.Collections.ObjectModel;
using BlockFactory.Side_;
using Mono.Cecil;

namespace BlockFactory.SideStripper;

[Test("LOLKEK")]
[ExclusiveTo(Side.Client)]
public class SideStripper
{
    private readonly Side _side;
    private readonly AssemblyDefinition[] _definitions;
    private readonly HashSet<TypeDefinition> _excludedTypes;
    private readonly HashSet<MethodDefinition> _excludedMethods;
    private readonly HashSet<FieldDefinition> _excludedFields;
    private readonly HashSet<PropertyDefinition> _excludedProperties;

    public SideStripper(Side side, AssemblyDefinition[] definitions)
    {
        _side = side;
        _definitions = definitions;
        _excludedTypes = new HashSet<TypeDefinition>();
        _excludedMethods = new HashSet<MethodDefinition>();
        _excludedFields = new HashSet<FieldDefinition>();
        _excludedProperties = new HashSet<PropertyDefinition>();
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
            var type in _definitions
                .SelectMany(a => a.Modules)
                .SelectMany(m => m.Types)
        )
        {
            if (IsExcluded(type.CustomAttributes))
            {
                Console.WriteLine($"Excluding type {type.FullName}");
                _excludedTypes.Add(type);
                continue;
            }
            foreach (var method in type.Methods.Where(method => IsExcluded(method.CustomAttributes)))
            {
                Console.WriteLine($"Excluding method {method.FullName}");
                _excludedMethods.Add(method);
            }
            foreach (var field in type.Fields.Where(field => IsExcluded(field.CustomAttributes)))
            {
                Console.WriteLine($"Excluding field {field.FullName}");
                _excludedFields.Add(field);
            }

            foreach (var property in type.Properties.Where(property => IsExcluded(property.CustomAttributes)))
            {
                Console.WriteLine($"Excluding property {property.FullName} and it's methods");
                _excludedProperties.Add(property);
                if (property.GetMethod != null)
                {
                    _excludedMethods.Add(property.GetMethod);
                }

                if (property.SetMethod != null)
                {
                    _excludedMethods.Add(property.SetMethod);
                }
            }
        }
    }

    private void RemoveExcludedThings()
    {
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