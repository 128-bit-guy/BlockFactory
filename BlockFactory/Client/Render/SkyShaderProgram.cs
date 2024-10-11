using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class SkyShaderProgram : ShaderProgram
{
    private readonly int _sunDirection;
    private readonly int _dayCoef;
    public SkyShaderProgram(string vertText, string fragText) : base(vertText, fragText)
    {
        _sunDirection = GetUniformLocation("sunDirection");
        _dayCoef = GetUniformLocation("dayCoef");
    }
    
    public void SetSunDirection(Vector3D<float> direction)
    {
        SetVector3(_sunDirection, direction);
    }

    public void SetDayCoef(float coef)
    {
        SetFloat(_dayCoef, coef);
    }
}