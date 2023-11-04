namespace BlockFactory.World_.Light;

public static class LightPropagator
{
    public static void ProcessLightUpdates(Chunk chunk)
    {
        DirectSkyLightPropagator.ProcessLightUpdates(chunk);
        DistanceLightPropagator.ProcessLightUpdates(chunk);
        foreach (var pos in chunk.ScheduledLightUpdates)
        {
            chunk.Data!.SetLightUpdateScheduled(pos, false);
        }
        chunk.ScheduledLightUpdates.Clear();
    }
}