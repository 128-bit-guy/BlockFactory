using System.Diagnostics.CodeAnalysis;
using BlockFactory.Registry_;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BlockFactory.Content.Biome_;

[SuppressMessage("Usage", "CA2211")]
public static class Biomes
{
    public static Registry<Biome> Registry;
    public static UndergroundBiome Underground;
    public static SurfaceBiome Surface;
    public static BigHeightBiome BigHeight;
    public static BeachBiome Beach;
    public static OceanBiome Ocean;

    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<Biome>("Biome");
        Underground = Registry.RegisterForced("Underground", 1, new UndergroundBiome());
        Surface = Registry.RegisterForced("Surface", 0, new SurfaceBiome());
        BigHeight = Registry.Register("BigHeight", new BigHeightBiome());
        Beach = Registry.Register("Beach", new BeachBiome());
        Ocean = Registry.Register("Ocean", new OceanBiome());
    }

    public static void Lock()
    {
        Registry.Lock();
    }
}