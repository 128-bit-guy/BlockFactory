namespace BlockFactory.Side_;

[AttributeUsage(AttributeTargets.All)]
public class ExclusiveToAttribute : Attribute
{
    public readonly Side Side;

    public ExclusiveToAttribute(Side side)
    {
        Side = side;
    }
}