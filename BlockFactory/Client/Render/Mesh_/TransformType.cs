using BlockFactory.Base;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public enum TransformType
{
    Position,
    Uv,
    Color,
    SpriteIndex,
    Light
}