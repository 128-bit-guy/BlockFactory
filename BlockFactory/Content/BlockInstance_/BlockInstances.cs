using System.Diagnostics.CodeAnalysis;
using BlockFactory.Registry_;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BlockFactory.Content.BlockInstance_;

[SuppressMessage("Usage", "CA2211")]
public static class BlockInstances
{
    public static Registry<BlockInstanceType> Registry;

    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<BlockInstanceType>("BlockInstanceType");
        
    }

    public static void Lock()
    {
        Registry.Lock();
    }
}