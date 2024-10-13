using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public static class LightInterpolation
{
    public static float GetInterpolatedBrightness(IBlockAccess world, Vector3D<double> pos, LightChannel channel)
    {
        var minBlockPos = (pos - new Vector3D<double>(0.5)).Floor();
        var relPos = pos - (minBlockPos.As<double>() + new Vector3D<double>(0.5));
        double sum = 0;
        for (var i = 0; i < 2; ++i)
        {
            var coefA = i == 0 ? 1 - relPos.X : relPos.X;
            for (var j = 0; j < 2; ++j)
            {
                var coefB =  coefA * (j == 0 ? 1 - relPos.Y : relPos.Y);
                for (var k = 0; k < 2; ++k)
                {
                    var curPos = minBlockPos + new Vector3D<int>(i, j, k);
                    if (!world.IsBlockLoaded(curPos))
                    {
                        return 1.0f;
                    }
                    var coefC =  coefB * (k == 0 ? 1 - relPos.Z : relPos.Z);
                    var light = world.GetLight(curPos, channel);
                    sum += coefC * (light / 15.0d);
                }
            }
        }

        return (float)sum;
    }

    public static float GetInterpolatedBrightness(IBlockAccess world, Vector3D<double> pos)
    {
        var sky = GetInterpolatedBrightness(world, pos, LightChannel.Sky);
        var block = GetInterpolatedBrightness(world, pos, LightChannel.Block);
        return MathF.Max(sky * world.GetDayCoefficient(), block);
    }
}