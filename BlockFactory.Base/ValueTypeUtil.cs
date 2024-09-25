namespace BlockFactory.Base;

public static class ValueTypeUtil
{
    public static T MakeDefault<T>() where T : struct
    {
        return default;
    }
}