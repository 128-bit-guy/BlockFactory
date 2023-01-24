namespace BlockFactory.Side_;

[AttributeUsage(AttributeTargets.Method)]
public class SidedEntryPointAttribute : Attribute
{
    public readonly Side Side;

    public SidedEntryPointAttribute(Side side)
    {
        Side = side;
    }
}