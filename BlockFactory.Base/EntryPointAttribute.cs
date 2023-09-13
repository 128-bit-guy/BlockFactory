namespace BlockFactory.Base;

[AttributeUsage(AttributeTargets.Method)]
public class EntryPointAttribute : Attribute
{
    public readonly Side Side;

    public EntryPointAttribute(Side side)
    {
        Side = side;
    }
}