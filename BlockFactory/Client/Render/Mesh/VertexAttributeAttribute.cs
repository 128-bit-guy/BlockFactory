namespace BlockFactory.Client.Render.Mesh;

[AttributeUsage(AttributeTargets.Field)]
public class VertexAttributeAttribute : Attribute
{
    public int LayoutLocation;
    public VertexAttributeAttribute(int layoutLocation)
    {
        LayoutLocation = layoutLocation;
    }
}