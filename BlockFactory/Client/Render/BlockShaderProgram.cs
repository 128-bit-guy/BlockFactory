using System.Drawing;
using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class BlockShaderProgram : ShaderProgram
{
    private readonly int _loadProgress;
    private readonly int _playerPos;
    private readonly int _skyColor;
    private readonly uint _spriteBoxesIndex;

    public BlockShaderProgram(string vertText, string fragText) : base(vertText, fragText)
    {
        _playerPos = GetUniformLocation("playerPos");
        _loadProgress = GetUniformLocation("loadProgress");
        _skyColor = GetUniformLocation("skyColor");
        _spriteBoxesIndex = 0;
    }

    public void SetPlayerPos(Vector3D<float> pos)
    {
        SetVector3(_playerPos, pos);
    }

    public void SetLoadProgress(float progress)
    {
        SetFloat(_loadProgress, progress);
    }

    public void SetSkyColor(Color color)
    {
        SetColor(_skyColor, color);
    }

    public void SetSpriteBoxesBinding(uint binding)
    {
        ShaderStorageBlockBinding(_spriteBoxesIndex, binding);
    }
}