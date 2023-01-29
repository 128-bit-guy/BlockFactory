using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.CubeMath;
using BlockFactory.Side_;
using BlockFactory.Util.Math_;

namespace BlockFactory.Client.Render.Mesh;

[ExclusiveTo(Side.Client)]
public static class VertexFormats
{

    public static VertexFormat<BlockVertex> Block { get; private set; } = null!;
    public static VertexFormat<GuiVertex> Gui { get; private set; } = null!;
    public static VertexFormat<ColorVertex> Color { get; private set; } = null!;
    internal static void Init()
    {
        Block = new VertexFormat<BlockVertex>(
            (v, m) => (m.Apply(v.Pos), v.Color, v.Uv),
            (v, p) => (v.Pos, v.Color * p, v.Uv),
            (v, l) => (v.Pos, v.Color, (v.Uv.X, v.Uv.Y, l))
        );
        Gui = new VertexFormat<GuiVertex>(
            (v, m) => (m.Apply(v.Pos), v.Color, v.Uv),
            (v, p) => (v.Pos, v.Color * p, v.Uv),
            (v, l) => v
        );
        Color = new VertexFormat<ColorVertex>(
            (v, m) => (m.Apply(v.Pos), v.Color),
            (v, p) => (v.Pos, v.Color * (p.X, p.Y, p.Z, 1.0f)),
            (v, l) => v
        );
    }
}