namespace BlockFactory.Base;

public static class Constants
{
    public static readonly TimeSpan TickPeriod = TimeSpan.FromMilliseconds(50);
    public static readonly int DefaultPort = 12345;
    public const int ChunkSizeLog2 = 4;
    public const int ChunkSize = 1 << ChunkSizeLog2;
    public const int ChunkMask = ChunkSize - 1;
}