namespace BlockFactory.Util;

public static class ExceptionUtils
{
    public static bool NotCancelledException(Exception exception)
    {
        return exception is not TaskCanceledException && (exception is not AggregateException aggregateException ||
                                                          !aggregateException.InnerExceptions.Any(o =>
                                                              o is TaskCanceledException));
    }
}