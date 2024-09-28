using BlockFactory.Base;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class TerrainShaderProgram : BlockShaderProgram
{
    private readonly int _skyTex;
    public TerrainShaderProgram(string vertText, string fragText) : base(vertText, fragText)
    {
        _skyTex = GetUniformLocation("skyTex");
    }

    public void SetSkyTex(int binding)
    {
        BfRendering.Gl.ProgramUniform1(Program, _skyTex, binding);
    }
}