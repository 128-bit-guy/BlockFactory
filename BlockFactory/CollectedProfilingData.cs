namespace BlockFactory;

public struct CollectedProfilingData
{
    public TimeSpan TotalTime;
    public int Cnt;

    public TimeSpan MeanTime => TotalTime / Cnt;
}