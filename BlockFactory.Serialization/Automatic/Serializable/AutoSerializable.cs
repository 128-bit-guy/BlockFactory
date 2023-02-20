namespace BlockFactory.Serialization.Automatic.Serializable;

public class AutoSerializable : IAutoSerializable
{
    public AutoSerializable() : this(SerializationManager.Common)
    {
    }

    public AutoSerializable(SerializationManager manager)
    {
        InitSerializer(manager);
    }

    [NotSerialized] public AutoSerializer AutoSerializer { get; private set; }

    public void InitSerializer(SerializationManager manager)
    {
        AutoSerializer = manager.GetSerializer(GetType());
    }
}