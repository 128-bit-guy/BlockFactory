using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Light;

public static class LightPropagator
{
    public static void ProcessLightUpdates(Chunk chunk)
    {
        DirectSkyLightPropagator.ProcessLightUpdates(chunk);

        DistanceLightPropagator.ProcessLightUpdates(chunk);
        foreach (var pos in chunk.ChunkUpdateInfo.ScheduledLightUpdates)
            chunk.Data!.SetLightUpdateScheduled(pos, false);

        chunk.ChunkUpdateInfo.ScheduledLightUpdates.Clear();
    }

    public static bool CanLightEnter(ChunkNeighbourhood neighbourhood, Vector3D<int> pos, CubeFace face,
        LightChannel channel)
    {
        var lightType = neighbourhood.GetLightType(pos);
        if (lightType == BlockLightType.Complex)
        {
            return neighbourhood.GetBlockObj(pos).CanLightLeave(face, channel);
        }
        else
        {
            return lightType == BlockLightType.Transparent ||
                   (lightType == BlockLightType.BlockingSky && channel != LightChannel.DirectSky);
        }
    }

    public static bool CanLightLeave(ChunkNeighbourhood neighbourhood, Vector3D<int> pos, CubeFace face,
        LightChannel channel)
    {
        return neighbourhood.GetLightType(pos) != BlockLightType.Complex ||
               neighbourhood.GetBlockObj(pos).CanLightLeave(face, channel);
    }
}