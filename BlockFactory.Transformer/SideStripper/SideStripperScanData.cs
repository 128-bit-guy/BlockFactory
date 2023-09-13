namespace BlockFactory.Transformer.SideStripper;

public class SideStripperScanData
{
    public readonly HashSet<string> ExcludedFields = new();
    public readonly HashSet<string> ExcludedMethods = new();
    public readonly HashSet<string> ExcludedProperties = new();
    public readonly HashSet<string> ExcludedTypes = new();
}