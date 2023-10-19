using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class BlockShaderProgram : ShaderProgram
{
    private readonly int _playerPos;
    public BlockShaderProgram(string vertText, string fragText) : base(vertText, fragText)
    {
        _playerPos = GetUniformLocation("playerPos");
    }

    public void SetPlayerPos(Vector3D<float> pos)
    {
        SetVector3(_playerPos, pos);
    }
}