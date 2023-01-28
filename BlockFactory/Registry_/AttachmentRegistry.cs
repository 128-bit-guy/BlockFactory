namespace BlockFactory.Registry_;

public class AttachmentRegistry<T1, T2>
    where T1 : IRegistryEntry
{
    private readonly Registry<T1> _registry;
    private List<T2> _attachments;
    public readonly List<T2> RegisteredAttachments;
    public AttachmentRegistry(Registry<T1> registry)
    {
        _registry = registry;
        _registry.OnSync += OnSync;
        _attachments = new List<T2>();
        RegisteredAttachments = new List<T2>();
    }

    private void OnSync(int[] permutation)
    {
        var newAttachments = new List<T2>(new T2[permutation.Length]);
        for (var i = 0; i < newAttachments.Count; ++i)
        {
            newAttachments[permutation[i]] = _attachments.Count > i ? _attachments[i] : default!;
        }

        _attachments = newAttachments;
    }

    public T3 Register<T3>(T1 obj, T3 attachment)
        where T3 : T2
    {
        var id = obj.Id;
        while (_attachments.Count <= id)
        {
            _attachments.Add(default!);
        }
        _attachments[id] = attachment;
        RegisteredAttachments.Add(attachment);
        return attachment;
    }

    public bool IsAttachmentAvailable(int id)
    {
        return id < _attachments.Count && _attachments[id] != null;
    }

    public virtual T2 this[int id] => _attachments[id];

    public T2 this[T1 obj] => this[obj.Id];

    public T2 this[RegistryName name] => this[_registry[name]];
}