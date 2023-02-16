namespace BlockFactory.Util;

public static class Algorithms
{
    public static void Shuffle<T>(this List<T> l, Random rng)
    {
        for (var i = 0; i < l.Count; ++i)
        {
            var j = rng.Next(l.Count);
            if (i != j) (l[i], l[j]) = (l[j], l[i]);
        }
    }

    public static void RemoveIf<T>(this List<T> l, Predicate<T> p)
    {
        var i = 0;
        for (var j = 0; j < l.Count; ++j)
            if (!p(l[j]))
                l[i++] = l[j];

        l.RemoveRange(i, l.Count - i);
    }
}