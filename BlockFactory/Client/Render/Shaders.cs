using BlockFactory.Base;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public static class Shaders
{
    public static BlockShaderProgram Block = null!;
    public static ShaderProgram Text = null!;
    public static ShaderProgram Gui = null!;
    public static ShaderProgram Sky = null!;
    public static TerrainShaderProgram Terrain = null!;

    public static void Init()
    {
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Block.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Block.Fragment.glsl")!;
            Block = new BlockShaderProgram(vertText, fragText);
        }
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Text.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Text.Fragment.glsl")!;
            Text = new ShaderProgram(vertText, fragText);
        }
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Gui.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Gui.Fragment.glsl")!;
            Gui = new ShaderProgram(vertText, fragText);
        }
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Sky.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Sky.Fragment.glsl")!;
            Sky = new ShaderProgram(vertText, fragText);
        }
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Terrain.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Assets.Shaders.Terrain.Fragment.glsl")!;
            Terrain = new TerrainShaderProgram(vertText, fragText);
        }
    }

    public static void Destroy()
    {
        Block.Dispose();
        Text.Dispose();
        Gui.Dispose();
        Sky.Dispose();
        Terrain.Dispose();
    }
}