namespace BlockFactory.Random_;

public class LinearCongruentialRandom : Random
{
    private long _seed;

    private const long Multiplier = 0x5DEECE66DL;
    private const long Addend = 0xBL;
    private const long Mask = (1L << 48) - 1;
    private const double DoubleUnit = 1 / ((double)(1L << 53));

    [ThreadStatic] private static LinearCongruentialRandom? _randomInstance;

    public static LinearCongruentialRandom ThreadLocalInstance => _randomInstance ??= new LinearCongruentialRandom();

    public LinearCongruentialRandom(long seed)
    {
        _seed = InitialScramble(seed);
    }

    public LinearCongruentialRandom() : this(Environment.TickCount64)
    {

    }


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
        return ((ulong)_seed) >> (48 - bits);
    }

    public override int Next()
    {
        return (int)NextBits(31);
    }

    public override int Next(int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentException();

        int r = (int)NextBits(31);
        int m = maxValue - 1;
        //if ((maxValue & m) == 0)
        //{
        //    Console.WriteLine("R : {0}", r);
        //    Console.WriteLine("MaxValue * R: {0}", (long)maxValue * (long)r);
        //    r = (int)((((long)maxValue) * ((long)r)) >> 31);
        //    Console.WriteLine("R became: {0}", r);
        //}
        //else
        {
            for (int u = r;
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
                 n-- > 0; rnd >>= 8)
                buffer[i++] = (byte)rnd;
    }

    public override double NextDouble()
    {
        ulong a = NextBits(26);
        ulong b = NextBits(27);
        ulong l = (a << 27) + b;
        double d = l * DoubleUnit;
        return d;
    }

    protected override double Sample()
    {
        return NextDouble();
    }
}