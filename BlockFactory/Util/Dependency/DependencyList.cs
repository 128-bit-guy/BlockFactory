namespace BlockFactory.Util.Dependency
{
    public class DependencyList : IDisposable
    {
        private List<IDependable> _dependencies;
        public DependencyList() { 
            _dependencies = new List<IDependable>();
        }

        public T Add<T>(T dep) where T : IDependable {
            dep.OnDependencyAdded();
            _dependencies.Add(dep);
            return dep;
        }

        public void Dispose()
        {
            foreach (IDependable dependable in _dependencies) {
                dependable.OnDependencyRemoved();
            }
        }
    }
}
