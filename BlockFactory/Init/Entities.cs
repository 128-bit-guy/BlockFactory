using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Registry_;

namespace BlockFactory.Init;

public static class Entities
{
    public static Registry<EntityType> Registry { get; private set; } = null!;
    public static EntityType Player { get; private set; } = null!;
    public static EntityType Item { get; private set; } = null!;
    public static void Init()
    {
        Registry = SyncedRegistries.NewSyncedRegistry<EntityType>(new RegistryName("Entities"));
        Player = Registry.Register(new RegistryName("Player"),
            new EntityType(() => new PlayerEntity(new PlayerInfo(new Credentials()))));
        Item = Registry.Register(new RegistryName("Item"), EntityType.Create<ItemEntity>());
        Registry.Lock();
    }
}