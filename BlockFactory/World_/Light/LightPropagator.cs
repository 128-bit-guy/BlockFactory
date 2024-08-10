namespace BlockFactory.World_.Light;

public static class LightPropagator
{
    public static void ProcessLightUpdates(Chunk chunk)
    {
        DirectSkyLightPropagator.ProcessLightUpdates(chunk);

        DistanceLightPropagator.ProcessLightUpdates(chunk);
        foreach (var pos in chunk.ChunkUpdateInfo.ScheduledLightUpdates) chunk.Data!.SetLightUpdateScheduled(pos, false);

        chunk.ChunkUpdateInfo.ScheduledLightUpdates.Clear();
    }
}