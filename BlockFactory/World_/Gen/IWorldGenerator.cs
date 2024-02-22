namespace BlockFactory.World_.Gen;

public interface IWorldGenerator
{
    public void GenerateChunk(Chunk c);
    public void DecorateChunk(Chunk c);
}