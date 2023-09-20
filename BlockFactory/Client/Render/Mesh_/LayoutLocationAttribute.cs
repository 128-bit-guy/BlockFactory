namespace BlockFactory.Client.Render.Mesh_;

[AttributeUsage(AttributeTargets.Field)]
public class LayoutLocationAttribute : Attribute
{
    public readonly uint Location;

    public LayoutLocationAttribute(uint location)
    {
        Location = location;
    }
}