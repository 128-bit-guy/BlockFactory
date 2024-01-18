namespace BlockFactory.World_.Light;

public static class LightPropagator
{
    public static void ProcessLightUpdates(Chunk chunk)
    {
        var init = !chunk.Data!.HasSkyLight;
        if (init)
            chunk.ProfileHelper.Start("Direct Sky Light");
        DirectSkyLightPropagator.ProcessLightUpdates(chunk);
        if (init)
        {
            chunk.ProfileHelper.Stop();
            chunk.ProfileHelper.Start("Distance Light");
        }

        DistanceLightPropagator.ProcessLightUpdates(chunk);
        if (init)
            chunk.ProfileHelper.Stop();
        foreach (var pos in chunk.ScheduledLightUpdates)
        {
            chunk.Data!.SetLightUpdateScheduled(pos, false);
        }

        chunk.ScheduledLightUpdates.Clear();
    }
}