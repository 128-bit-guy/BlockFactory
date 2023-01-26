using BlockFactory.Resource;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Render.Shader;

[ExclusiveTo(Side.Client)]
public class ColorShaderProgram : ShaderProgram
{
    public int ColorUniform;

    public ColorShaderProgram(string vertexPath, string fragmentPath, IResourceLoader loader) : base(vertexPath,
        fragmentPath, loader)
    {
    }

    public ColorShaderProgram(string pathBegin, IResourceLoader loader) : base(pathBegin, loader)
    {
    }

    protected override void LoadUniforms()
    {
        base.LoadUniforms();
        ColorUniform = GL.GetUniformLocation(Program, "uColor");
    }

    public void SetColor(Vector4 color)
    {
        GL.ProgramUniform4(Program, ColorUniform, color);
        // GL.Uniform4(ColorUniform, color);
    }
}