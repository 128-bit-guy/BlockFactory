namespace BlockFactory.CubeMath;

public static class Algorithms
{
    public static bool NextPermutation<T>(IList<T> a) where T : IComparable<T>
    {
        if (a.Count < 2) return false;
        var k = a.Count - 2;

        while (k >= 0 && a[k].CompareTo(a[k + 1]) >= 0) k--;
        if (k < 0) return false;

        var l = a.Count - 1;
        while (l > k && a[l].CompareTo(a[k]) <= 0) l--;

        var tmp = a[k];
        a[k] = a[l];
        a[l] = tmp;

        var i = k + 1;
        var j = a.Count - 1;
        while (i < j)
        {
            tmp = a[i];
            a[i] = a[j];
            a[j] = tmp;
            i++;
            j--;
        }

        return true;
    }
}