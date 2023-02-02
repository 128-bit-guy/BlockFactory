using BlockFactory.Registry_;

namespace BlockFactory.Init;

public class SyncedRegistries
{
    private static Dictionary<RegistryName, IRegistry> SyncedRegistryDictionary = null!;

    public static Registry<T> NewSyncedRegistry<T>(RegistryName name) where T : IRegistryEntry
    {
        Registry<T> registry = new(name);
        SyncedRegistryDictionary.Add(name, registry);
        return registry;
    }

    public static void Init()
    {
        SyncedRegistryDictionary = new Dictionary<RegistryName, IRegistry>();
    }

    public static void Sync((RegistryName, RegistryName[])[] data)
    {
        foreach (var (name, order) in data)
        {
            Console.WriteLine("Synchronizing registry {0}", name);
            SyncedRegistryDictionary[name].Synchronize(order);
        }
    }

    public static (RegistryName, RegistryName[])[] GetSyncData()
    {
        List<(RegistryName, RegistryName[])> list = new();
        foreach (var (name, registry) in SyncedRegistryDictionary) list.Add((name, registry.GetNameOrder()));
        return list.ToArray();
    }
}