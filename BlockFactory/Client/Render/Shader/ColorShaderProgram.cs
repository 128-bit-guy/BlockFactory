using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Shader;

public class ColorShaderProgram : ShaderProgram
{
    public int ColorUniform;
    public ColorShaderProgram(string vertexPath, string fragmentPath) : base(vertexPath, fragmentPath)
    {

    }
    public ColorShaderProgram(string pathBegin) : base(pathBegin)
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