namespace BlockFactory.Client.Render.Mesh_;

[AttributeUsage(AttributeTargets.Field)]
public class TransformationTypeAttribute : Attribute
{
    public readonly TransformType TransformType;

    public TransformationTypeAttribute(TransformType transformType)
    {
        TransformType = transformType;
    }
}