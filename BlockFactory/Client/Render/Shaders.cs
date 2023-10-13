using BlockFactory.Base;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public static class Shaders
{
    public static ShaderProgram Block = null!;


    public static void Init()
    {
        var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Vertex.glsl")!;
        var fragText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Fragment.glsl")!;
        Block = new ShaderProgram(vertText, fragText);
    }

    public static void Destroy()
    {
        Block.Dispose();
    }
}