namespace BlockFactory.World_.Gen;

public interface IWorldGenerator
{
    public long Seed { get; }
    public void GenerateChunk(Chunk c);
    public void DecorateChunk(Chunk c);
}