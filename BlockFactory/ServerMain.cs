using BlockFactory.Base;

namespace BlockFactory;

[ExclusiveTo(Side.Server)]
public static class ServerMain
{
    public static object x = 228;

    [EntryPoint(Side.Server)]
    public static void Main()
    {
        Console.WriteLine("Launching Server!");
    }
}