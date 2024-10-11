using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public static class LightInterpolation
{
    public static double GetInterpolatedBrightness(IBlockAccess world, Vector3D<double> pos)
    {
        var minBlockPos = (pos - new Vector3D<double>(0.5)).Floor();
        var relPos = pos - (minBlockPos.As<double>() + new Vector3D<double>(0.5));
        var dayCoef = world.GetDayCoefficient();
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
                        return 1.0;
                    }
                    var coefC =  coefB * (k == 0 ? 1 - relPos.Z : relPos.Z);
                    var light = Math.Max((double)world.GetLight(curPos, LightChannel.Sky) * dayCoef,
                        world.GetLight(curPos, LightChannel.Block));
                    sum += coefC * (light / 15);
                }
            }
        }

        return sum;
    }
}