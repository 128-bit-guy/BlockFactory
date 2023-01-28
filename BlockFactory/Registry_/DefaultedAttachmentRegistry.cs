namespace BlockFactory.Registry_;

public class DefaultedAttachmentRegistry <T1, T2> : AttachmentRegistry<T1, T2>
    where T1 : IRegistryEntry
{
    public readonly T2 Default;
    public DefaultedAttachmentRegistry(Registry<T1> registry, T2 d) : base(registry)
    {
        Default = d;
    }

    public override T2 this[int id] => IsAttachmentAvailable(id)? base[id] : Default;
}