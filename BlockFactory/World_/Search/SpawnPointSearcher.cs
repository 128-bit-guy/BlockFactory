using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_.Search;

public class SpawnPointSearcher : WorldLocationSearcher
{
    public SpawnPointSearcher(IChunkStorage world) : base(world)
    {
    }

    protected override Vector3D<int> GetPotentialChunkPos()
    {
        var x = Random.Shared.Next(-16, 17);
        var y = Random.Shared.Next(-1, 2);
        var z = Random.Shared.Next(-16, 17);
        return new Vector3D<int>(x, y, z);
    }

    protected override bool CheckPosition(BlockPointer pos)
    {
        if (pos.GetBlock() != 0)
        {
            return false;
        } 
        if ((pos + CubeFace.Bottom.GetDelta()).GetBlock() != 0)
        {
            return false;
        }
        if (!(pos + 2 * CubeFace.Bottom.GetDelta()).GetBlockObj().IsFaceSolid(CubeFace.Top))
        {
            return false;
        }

        if (pos.GetLight(LightChannel.Sky) < 10)
        {
            return false;
        }
        return true;
    }
}