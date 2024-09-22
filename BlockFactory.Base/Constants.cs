namespace BlockFactory.Base;

public static class Constants
{
    public const string Name = "BlockFactory";
    public const int ChunkSizeLog2 = 4;
    public const int ChunkSize = 1 << ChunkSizeLog2;
    public const int ChunkMask = ChunkSize - 1;
    public const int ChunkArrayLength = ChunkSize * ChunkSize * ChunkSize;
    public const int TickFrequencyMs = 50;
    public const int DefaultPort = 12345;
}