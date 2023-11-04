namespace BlockFactory;

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
}