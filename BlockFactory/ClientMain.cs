using BlockFactory.Base;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory;

[ExclusiveTo(Side.Client)]
public static class ClientMain
{
    [EntryPoint(Side.Client)]
    public static void Main()
    {
        Console.WriteLine("Launching Client!");
        Console.WriteLine(ServerMain.x);
        GLFW.Init();
    }
}