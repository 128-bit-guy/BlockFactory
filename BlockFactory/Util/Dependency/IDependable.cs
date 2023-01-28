namespace BlockFactory.Util.Dependency
{
    public interface IDependable
    {
        ref int DependencyCount { get; }

        void OnDependencyAdded()
        {
            ++DependencyCount;
        }

        void OnDependencyRemoved()
        {
            --DependencyCount;
        }
    }
}
