using System.Collections;

namespace BlockFactory.Registry_;

public class Registry<T> : IRegistry, IEnumerable<T> where T : class, IRegistryEntry
{
    public delegate void RegisterHandler(string id, T entry);
    private readonly Dictionary<string, T> _entries = new();
    private readonly Dictionary<string, int> _forcedIds = new();
    private readonly HashSet<int> _occupiedForcedIds = new();
    private readonly List<RegisterHandler> _handlers = new();

    private T?[]? _entriesByNumId;
    private string?[]? _stringIdsByNumId;

    public bool Locked { get; private set; }

    public T this[string id] => _entries[id];

    public T? this[int id] => _entriesByNumId![id];

    public IEnumerator<T> GetEnumerator()
    {
        return _entries.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _entries.Values.GetEnumerator();
    }

    public void AssignNumericalIds(Dictionary<string, int> mapping)
    {
        if (!Locked) throw new InvalidOperationException("Registry must be locked before assigning numerical ids");

        foreach (var (s, i) in _forcedIds)
            if (mapping.TryGetValue(s, out var oi) && oi != i)
                throw new ArgumentException(
                    $"Object with id {s} with forced id {i} has different id {oi} in mapping"
                );

        var arrSize = _entries.Count;

        if (_forcedIds.Count > 0) arrSize = Math.Max(arrSize, _forcedIds.Values.Max() + 1);

        if (mapping.Count > 0) arrSize = Math.Max(arrSize, mapping.Values.Max() + 1);

        _entriesByNumId = new T?[arrSize];
        _stringIdsByNumId = new string?[arrSize];

        foreach (var (s, i) in mapping)
        {
            _entriesByNumId[i] = _entries[s];
            _stringIdsByNumId[i] = s;
            _entries[s].Id = i;
        }

        foreach (var (s, i) in _forcedIds)
        {
            if (mapping.ContainsKey(s)) continue;
            _entriesByNumId[i] = _entries[s];
            _stringIdsByNumId[i] = s;
            _entries[s].Id = i;
        }

        var pos = 0;
        foreach (var (s, t) in _entries)
        {
            if (mapping.ContainsKey(s) || _forcedIds.ContainsKey(s)) continue;

            while (_entriesByNumId[pos] != null) ++pos;

            _entriesByNumId[pos] = t;
            _stringIdsByNumId[pos] = s;
            t.Id = pos;
        }
    }

    public Dictionary<string, int> GetStringToNumIdMapping()
    {
        var res = new Dictionary<string, int>();
        foreach (var entry in _entries.Values) res[GetStringId(entry)] = entry.Id;

        return res;
    }

    public T1 Register<T1>(string id, T1 entry) where T1 : T
    {
        if (Locked)
            throw new InvalidOperationException(
                $"Registry is locked when trying to register object with id {id}"
            );

        if (_entries.ContainsKey(id)) throw new ArgumentException($"Registry already contains object with id {id}");

        _entries.Add(id, entry);

        foreach (var handler in _handlers)
        {
            handler(id, entry);
        }

        return entry;
    }

    public T1 RegisterForced<T1>(string id, int numId, T1 entry) where T1 : T
    {
        if (_occupiedForcedIds.Contains(numId))
            throw new ArgumentException($"Registry already contains object with forced numerical id {numId}"
                                        + $" when trying to register object with id {id}");

        Register(id, entry);

        _forcedIds.Add(id, numId);

        _occupiedForcedIds.Add(numId);

        return entry;
    }

    public void Lock()
    {
        Locked = true;
    }

    public string GetStringId(T entry)
    {
        return _stringIdsByNumId![entry.Id]!;
    }

    public int? GetForcedId(string name)
    {
        if (_forcedIds.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }

    public void ForEachEntry(RegisterHandler handler)
    {
        foreach (var (id, entry) in _entries)
        {
            handler(id, entry);
        }
        _handlers.Add(handler);
    }

    public int Count => _entries.Count;
}