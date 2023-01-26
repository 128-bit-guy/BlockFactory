using BlockFactory.Resource;
using BlockFactory.Side_;

namespace BlockFactory.Render.Shader;

[ExclusiveTo(Side.Client)]
public class Shaders : IDisposable
{
    private readonly IResourceLoader _loader;

    public Shaders(IResourceLoader loader)
    {
        _loader = loader;
    }

    public ShaderProgram Block { get; private set; } = null!;
    public ShaderProgram Text { get; private set; } = null!;
    public ShaderProgram Gui { get; private set; } = null!;
    public ColorShaderProgram Color { get; private set; } = null!;

    public void Dispose()
    {
        Block.Dispose();
        Text.Dispose();
        Gui.Dispose();
        Color.Dispose();
    }

    public void Init()
    {
        Block = new ShaderProgram("BlockFactory.Assets.Shaders.block", _loader);
        Text = new ShaderProgram("BlockFactory.Assets.Shaders.text", _loader);
        Gui = new ShaderProgram("BlockFactory.Assets.Shaders.gui", _loader);
        Color = new ColorShaderProgram("BlockFactory.Assets.Shaders.color", _loader);
    }
}