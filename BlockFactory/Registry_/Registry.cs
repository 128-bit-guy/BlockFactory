namespace BlockFactory.Registry_;

public class Registry<T> : IRegistry
    where T : IRegistryEntry
{
    public delegate void SyncHandler(int[] permutation);

    public readonly RegistryName Name;
    private List<T> _entries;
    private List<RegistryName> _names;
    private Dictionary<RegistryName, T>? _objects;

    public Registry(RegistryName name)
    {
        _entries = new List<T>();
        _names = new List<RegistryName>();
        Name = name;
    }

    public bool Locked { get; private set; }

    public T this[RegistryName name]
    {
        get
        {
            if (Locked)
                return _objects![name];
            throw new InvalidOperationException("Registry is not locked");
        }
    }

    public T this[int id]
    {
        get
        {
            if (Locked)
                return _entries[id];
            throw new InvalidOperationException("Registry is not locked");
        }
    }

    public void Synchronize(RegistryName[] order)
    {
        if (Locked)
        {
            var permutation = BuildPermutation(order);
            OnSync(permutation);
            List<T> newEntries = new(new T[_entries.Count]);
            List<RegistryName> newNames = new(new RegistryName[_names.Count]);
            for (var i = 0; i < _entries.Count; ++i)
            {
                newEntries[permutation[i]] = _entries[i];
                newNames[permutation[i]] = _names[i];
            }

            _entries = newEntries;
            _names = newNames;
        }
        else
        {
            throw new InvalidOperationException("Registry is not locked");
        }
    }

    public RegistryName[] GetNameOrder()
    {
        if (Locked)
            return _names.ToArray();
        throw new InvalidOperationException("Registry is not locked");
    }

    public event SyncHandler OnSync = _ => { };

    public T2 Register<T2>(RegistryName name, T2 obj) where T2 : T
    {
        if (Locked)
        {
            throw new InvalidOperationException("Registry is locked");
        }

        obj.Id = _entries.Count;
        _entries.Add(obj);
        _names.Add(name);
        return obj;
    }

    public void Lock()
    {
        if (Locked)
        {
            throw new InvalidOperationException("Registry is already locked");
        }

        Locked = true;
        _objects = new Dictionary<RegistryName, T>();
        for (var i = 0; i < _entries.Count; ++i) _objects[_names[i]] = _entries[i];
    }

    public RegistryName GetName(T obj)
    {
        if (Locked)
            return _names[obj.Id];
        throw new InvalidOperationException("Registry is not locked");
    }

    private int[] BuildPermutation(RegistryName[] order)
    {
        Dictionary<RegistryName, int> poses = new();
        for (var i = 0; i < order.Length; ++i) poses[order[i]] = i;
        var cNewPos = order.Length;
        var permutation = new int[_names.Count];
        for (var i = 0; i < _names.Count; ++i)
        {
            var name = _names[i];
            if (poses.TryGetValue(name, out var index))
                permutation[i] = index;
            else
                permutation[i] = cNewPos++;
        }

        return permutation;
    }

    public List<T> GetRegisteredEntries()
    {
        return _entries;
    }
}