using BlockFactory.Base;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
[AttributeUsage(AttributeTargets.Field)]
public class TransformationTypeAttribute : Attribute
{
    public readonly TransformType TransformType;

    public TransformationTypeAttribute(TransformType transformType)
    {
        TransformType = transformType;
    }
}