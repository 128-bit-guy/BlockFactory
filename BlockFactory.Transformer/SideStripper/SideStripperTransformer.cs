using BlockFactory.Base;
using Mono.Cecil;

namespace BlockFactory.Transformer.SideStripper;

public partial class SideStripperTransformer : AssemblyTransformer<SideStripperScanData>
{
    private readonly Side _side;

    public SideStripperTransformer(Side side)
    {
        _side = side;
    }

    private bool IsExcluded(IEnumerable<CustomAttribute> attributes)
    {
        return attributes
            .Where(attr => attr.AttributeType.FullName == typeof(ExclusiveToAttribute).FullName)
            .Any(attr => (Side)attr.ConstructorArguments[0].Value != _side);
    }

    protected override SideStripperScanData ScanAssemblyInternal(AssemblyDefinition definition)
    {
        var data = new SideStripperScanData();
        foreach (var type in definition.AllTypes())
        {
            if (IsExcluded(type.CustomAttributes))
            {
                Console.WriteLine($"Excluding type {type.FullName}");
                data.ExcludedTypes.Add(type.FullName);
                continue;
            }

            foreach (var method in type.Methods.Where(method => IsExcluded(method.CustomAttributes)))
            {
                Console.WriteLine($"Excluding method {method.FullName}");
                data.ExcludedMethods.Add(method.FullName);
            }

            foreach (var field in type.Fields.Where(field => IsExcluded(field.CustomAttributes)))
            {
                Console.WriteLine($"Excluding field {field.FullName}");
                data.ExcludedFields.Add(field.FullName);
            }

            foreach (var property in type.Properties.Where(property => IsExcluded(property.CustomAttributes)))
            {
                Console.WriteLine($"Excluding property {property.FullName} and it's methods");
                data.ExcludedProperties.Add(property.FullName);
                if (property.GetMethod != null) data.ExcludedMethods.Add(property.GetMethod.FullName);

                if (property.SetMethod != null) data.ExcludedMethods.Add(property.SetMethod.FullName);

                var field = property.GetBackingField();
                if (field != null) data.ExcludedFields.Add(field.FullName);
            }
        }

        return data;
    }

    private static bool ScanDataContains(Func<string, SideStripperScanData?> scanDataGetter, FieldReference definition)
    {
        var data = scanDataGetter(definition.Module.Assembly.FullName);
        return data?.ExcludedFields?.Contains(definition.FullName) ?? false;
    }

    private static bool ScanDataContains(Func<string, SideStripperScanData?> scanDataGetter,
        MethodReference definition)
    {
        var data = scanDataGetter(definition.Module.Assembly.FullName);
        return data?.ExcludedMethods?.Contains(definition.FullName) ?? false;
    }

    private static bool ScanDataContains(Func<string, SideStripperScanData?> scanDataGetter,
        PropertyReference definition)
    {
        var data = scanDataGetter(definition.Module.Assembly.FullName);
        return data?.ExcludedProperties?.Contains(definition.FullName) ?? false;
    }

    private static bool ScanDataContains(Func<string, SideStripperScanData?> scanDataGetter, TypeReference definition)
    {
        var data = scanDataGetter(definition.Module.Assembly.FullName);
        return data?.ExcludedTypes?.Contains(definition.FullName) ?? false;
    }

    protected override void TransformAssemblyInternal(AssemblyDefinition def,
        Func<string, SideStripperScanData?> scanDataGetter)
    {
        FixThings(def, scanDataGetter);
        RemoveExcludedThings(def, scanDataGetter);
    }
}