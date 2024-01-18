using System.Diagnostics;

namespace BlockFactory;

public class ProfileHelper
{
    public readonly Dictionary<string, TimeSpan> Time;
    private readonly Stopwatch _stopwatch;
    private string? _curType;

    public ProfileHelper()
    {
        Time = new Dictionary<string, TimeSpan>();
        _stopwatch = new Stopwatch();
        _curType = null;
    }

    public void Start(string s)
    {
        _curType = s;
        _stopwatch.Restart();
    }

    public void Stop()
    {
        _stopwatch.Stop();
        if (Time.TryGetValue(_curType!, out var time))
        {
            Time[_curType!] = time + _stopwatch.Elapsed;
        }
        else
        {
            Time[_curType!] = _stopwatch.Elapsed;
        }

        _curType = null;
    }
}