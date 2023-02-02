using BlockFactory.Side_;

namespace BlockFactory.Client.Render.Shader;

[ExclusiveTo(Side.Client)]
public static class Shaders
{
    public static ShaderProgram Block { get; private set; } = null!;
    public static ShaderProgram Text { get; private set; } = null!;
    public static ShaderProgram Gui { get; private set; } = null!;
    public static ColorShaderProgram Color { get; private set; } = null!;

    internal static void Init()
    {
        Block = new ShaderProgram("BlockFactory.Assets.Shaders.block");
        Text = new ShaderProgram("BlockFactory.Assets.Shaders.text");
        Gui = new ShaderProgram("BlockFactory.Assets.Shaders.gui");
        Color = new ColorShaderProgram("BlockFactory.Assets.Shaders.color");
    }
}