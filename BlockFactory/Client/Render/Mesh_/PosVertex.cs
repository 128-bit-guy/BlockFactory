using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public struct PosVertex
{
    [LayoutLocation(0)] [TransformationType(TransformType.Position)]
    public Vector3D<float> Pos;

    public PosVertex(Vector3D<float> pos)
    {
        Pos = pos;
    }
    
    public PosVertex(float x, float y, float z) : this(new Vector3D<float>(x, y, z))
    {
        
    }
}