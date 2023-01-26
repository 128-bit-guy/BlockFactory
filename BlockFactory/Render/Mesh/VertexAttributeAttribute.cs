using BlockFactory.Side_;

namespace BlockFactory.Render.Mesh;

[ExclusiveTo(Side.Client)]
[AttributeUsage(AttributeTargets.Field)]
public class VertexAttributeAttribute : Attribute
{
    public int LayoutLocation;

    public VertexAttributeAttribute(int layoutLocation)
    {
        LayoutLocation = layoutLocation;
    }
}