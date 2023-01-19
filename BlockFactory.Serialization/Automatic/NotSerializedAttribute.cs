namespace BlockFactory.Serialization.Automatic;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NotSerializedAttribute : Attribute
{
}