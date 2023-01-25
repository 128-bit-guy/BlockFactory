namespace BlockFactory.Serialization.Automatic;

public class RangeAttribute : Attribute
{
    public bool FloatingPoint;
    public long Min, Max;
    public double MinFP, MaxFP;

    public RangeAttribute(long min, long max)
    {
        Min = min;
        Max = max;
        FloatingPoint = false;
    }

    public RangeAttribute(double min, double max)
    {
        MinFP = min;
        MaxFP = max;
        FloatingPoint = true;
    }
}