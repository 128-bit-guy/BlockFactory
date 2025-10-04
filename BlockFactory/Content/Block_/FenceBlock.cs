using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Physics;
using BlockFactory.World_;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Content.Block_;

public class FenceBlock : Block
{
    private Box3D<double> _center;
    private Box3D<double>[] _connections;

    public FenceBlock()
    {
        double halfWidth = 4 / 32.0f;
        double centerMin = 0.5 - halfWidth;
        double centerMax = 0.5 + halfWidth;
        _center = new Box3D<double>(new Vector3D<double>(centerMin, 0.0f, centerMin),
            new Vector3D<double>(centerMax, 1.0f, centerMax));
        var front = new Box3D<double>(new Vector3D<double>(centerMin, 0.0f, centerMax),
            new Vector3D<double>(centerMax, 1.0f, 1.0f));
        _connections = new Box3D<double>[6];
        foreach (var face in CubeFaceUtils.Values())
        {
            if(face.GetAxis() == 1) continue;
            var symmetry = CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;
            var curConnection = symmetry.TransformAroundCenter(front.As<float>()).As<double>();
            _connections[(int)face] = curConnection;
        }
    }

    public override int GetTexture(CubeFace face)
    {
        return 11;
    }

    public override bool CanLightEnter(CubeFace face, LightChannel channel)
    {
        return true;
    }

    public override bool CanLightLeave(CubeFace face, LightChannel channel)
    {
        return true;
    }

    [ExclusiveTo(Side.Client)]
    public override bool HasAo()
    {
        return false;
    }

    public override bool IsFaceSolid(CubeFace face)
    {
        return false;
    }

    [ExclusiveTo(Side.Client)]
    public override bool BlockRendering(CubeFace face)
    {
        return false;
    }

    public virtual bool Connects(ConstBlockPointer pointer, CubeFace face)
    {
        var oPointer = pointer + face.GetDelta();
        if (oPointer.GetBlockObj().IsFaceSolid(face.GetOpposite()))
        {
            return true;
        }

        return oPointer.GetBlockObj().Id == Id;
    }

    public override void AddBlockBoxes(ConstBlockPointer pointer, BoxConsumer.BoxConsumerFunc consumer,
        BlockBoxType type)
    {
        consumer(_center);
        foreach (var face in CubeFaceUtils.Values())
        {
            if(face.GetAxis() == 1) continue;
            if(!Connects(pointer, face)) continue;
            consumer(_connections[(int)face]);
        }
    }
}