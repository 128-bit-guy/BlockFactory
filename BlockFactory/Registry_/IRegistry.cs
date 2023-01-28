namespace BlockFactory.Registry_
{
    internal interface IRegistry
    {
        public void Synchronize(RegistryName[] order);
        public RegistryName[] GetNameOrder();
    }
}
