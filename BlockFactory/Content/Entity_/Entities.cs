using System.Diagnostics.CodeAnalysis;
using BlockFactory.Registry_;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BlockFactory.Content.Entity_;

[SuppressMessage("Usage", "CA2211")]
public static class Entities
{
    public static Registry<EntityType> Registry;
    public static EntityType Player;

    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<EntityType>("EntityType");
        Player = Registry.RegisterForced("Player", 0, new EntityType(PlayerEntity.Create));
    }

    public static void Lock()
    {
        Registry.Lock();
    }
}