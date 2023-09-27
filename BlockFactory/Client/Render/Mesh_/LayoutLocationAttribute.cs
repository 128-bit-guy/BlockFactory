using BlockFactory.Base;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
[AttributeUsage(AttributeTargets.Field)]
public class LayoutLocationAttribute : Attribute
{
    public readonly uint Location;

    public LayoutLocationAttribute(uint location)
    {
        Location = location;
    }
}