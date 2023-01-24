namespace BlockFactory.SideStripper;

[AttributeUsage(AttributeTargets.Class)]
public class TestAttribute : Attribute
{
    public readonly string Value;

    public TestAttribute(string value)
    {
        Value = value;
    }
}