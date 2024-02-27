namespace BlockFactory.Registry_;

public static class SynchronizedRegistries
{
    private static readonly Dictionary<string, IRegistry> SyncedRegistries = new();

    public static Registry<T> NewSynchronizedRegistry<T>(string id) where T : class, IRegistryEntry
    {
        var result = new Registry<T>();
        SyncedRegistries.Add(id, result);
        return result;
    }

    public static void LoadMapping(RegistryMapping mapping)
    {
        foreach (var (id, reg) in SyncedRegistries)
            if (mapping.Mappings.TryGetValue(id, out var m))
                reg.AssignNumericalIds(m);
            else
                reg.AssignNumericalIds(new Dictionary<string, int>());
    }

    public static RegistryMapping WriteMapping()
    {
        var mapping = new RegistryMapping();
        foreach (var (id, reg) in SyncedRegistries)
        {
            var m = reg.GetStringToNumIdMapping();
            mapping.Mappings.Add(id, m);
        }

        return mapping;
    }
}