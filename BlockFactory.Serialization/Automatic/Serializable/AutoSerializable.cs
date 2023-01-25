namespace BlockFactory.Serialization.Automatic.Serializable;

public class AutoSerializable : IAutoSerializable
{
    public AutoSerializable()
    {
        AutoSerializer = null!;
    }

    public AutoSerializable(SerializationManager manager) : this()
    {
        InitSerializer(manager);
    }

    [NotSerialized] public AutoSerializer AutoSerializer { get; private set; }

    public void InitSerializer(SerializationManager manager)
    {
        AutoSerializer = manager.GetSerializer(GetType());
    }
}