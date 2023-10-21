namespace BlockFactory.Random_;

public class LinearCongruentialRandom : Random
{
    private const long Multiplier = 0x5DEECE66DL;
    private const long Addend = 0xBL;
    private const long Mask = (1L << 48) - 1;
    private const double DoubleUnit = 1 / (double)(1L << 53);

    [ThreadStatic] private static LinearCongruentialRandom? _randomInstance;
    private long _seed;

    public LinearCongruentialRandom(long seed)
    {
        _seed = InitialScramble(seed);
    }

    public LinearCongruentialRandom() : this(Environment.TickCount64)
    {
    }

    public static LinearCongruentialRandom ThreadLocalInstance => _randomInstance ??= new LinearCongruentialRandom();


    private static long InitialScramble(long seed)
    {
        return (seed ^ Multiplier) & Mask;
    }

    public void SetSeed(long seed)
    {
        _seed = InitialScramble(seed);
    }

    private ulong NextBits(int bits)
    {
        _seed = (_seed * Multiplier + Addend) & Mask;
        return (ulong)_seed >> (48 - bits);
    }

    public override int Next()
    {
        return (int)NextBits(31);
    }

    public override int Next(int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentException();

        var r = (int)NextBits(31);
        var m = maxValue - 1;
        //if ((maxValue & m) == 0)
        //{
        //    Console.WriteLine("R : {0}", r);
        //    Console.WriteLine("MaxValue * R: {0}", (long)maxValue * (long)r);
        //    r = (int)((((long)maxValue) * ((long)r)) >> 31);
        //    Console.WriteLine("R became: {0}", r);
        //}
        //else
        {
            for (var u = r;
                 u - (r = u % maxValue) + m < 0;
                 u = (int)NextBits(31))
                ;
        }
        return r;
    }

    public override int Next(int minValue, int maxValue)
    {
        return Next(maxValue - minValue) + minValue;
    }

    public override void NextBytes(byte[] buffer)
    {
        NextBytes(buffer.AsSpan());
    }

    public override void NextBytes(Span<byte> buffer)
    {
        for (int i = 0, len = buffer.Length; i < len;)
        for (int rnd = Next(),
             n = Math.Min(len - i, sizeof(int));
             n-- > 0;
             rnd >>= 8)
            buffer[i++] = (byte)rnd;
    }

    public override double NextDouble()
    {
        var a = NextBits(26);
        var b = NextBits(27);
        var l = (a << 27) + b;
        var d = l * DoubleUnit;
        return d;
    }

    protected override double Sample()
    {
        return NextDouble();
    }
}