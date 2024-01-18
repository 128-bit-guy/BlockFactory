using Silk.NET.Input;

namespace BlockFactory;

public class ProfileCollector
{
    public Dictionary<string, CollectedProfilingData> ProfilingResults;

    public ProfileCollector()
    {
        ProfilingResults = new Dictionary<string, CollectedProfilingData>();
    }

    public void AddHelper(ProfileHelper helper)
    {
        foreach (var (key, val) in helper.Time)
        {
            var oldData = ProfilingResults.GetValueOrDefault(key);
            ++oldData.Cnt;
            oldData.TotalTime += val;
            ProfilingResults[key] = oldData;
        }
    }

    public void Clear()
    {
        ProfilingResults.Clear();
    }
}