using Mono.Cecil;

namespace BlockFactory.Transformer.SideStripper;

public partial class SideStripperTransformer
{
    private static void RemoveExcludedThingsFromType(TypeDefinition t,
        Func<string, SideStripperScanData?> scanDataGetter)
    {
        t.Fields.RemoveIf(f => ScanDataContains(scanDataGetter, f));
        t.Methods.RemoveIf(m => ScanDataContains(scanDataGetter, m));
        t.Properties.RemoveIf(p => ScanDataContains(scanDataGetter, p));
        t.NestedTypes.RemoveIf(t => ScanDataContains(scanDataGetter, t));
        foreach (var nestedType in t.NestedTypes) RemoveExcludedThingsFromType(nestedType, scanDataGetter);
    }

    private static void RemoveExcludedThings(AssemblyDefinition def, Func<string, SideStripperScanData?> scanDataGetter)
    {
        foreach (var m in def.Modules)
        {
            m.Types.RemoveIf(t => ScanDataContains(scanDataGetter, t));
            foreach (var type in m.Types) RemoveExcludedThingsFromType(type, scanDataGetter);
        }
    }
}