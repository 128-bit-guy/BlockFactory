namespace BlockFactory.Util.Dependency;

public class DependencyList : IDisposable
{
    private readonly List<IDependable> _dependencies;

    public DependencyList()
    {
        _dependencies = new List<IDependable>();
    }

    public void Dispose()
    {
        foreach (var dependable in _dependencies) dependable.OnDependencyRemoved();
    }

    public T Add<T>(T dep) where T : IDependable
    {
        dep.OnDependencyAdded();
        _dependencies.Add(dep);
        return dep;
    }
}