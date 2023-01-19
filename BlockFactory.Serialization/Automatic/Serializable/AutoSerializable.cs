namespace BlockFactory.Serialization.Automatic.Serializable;

public class AutoSerializable : IAutoSerializable
{
    [NotSerialized] public AutoSerializer AutoSerializer { get; private set; }

    public AutoSerializable()
    {
        AutoSerializer = null!;
    }

    public AutoSerializable(SerializationManager manager) : this()
    {
        InitSerializer(manager);
    }

    public void InitSerializer(SerializationManager manager)
    {
        AutoSerializer = manager.GetSerializer(GetType());
    }
}