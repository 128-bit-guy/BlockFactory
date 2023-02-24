namespace BlockFactory.Util;

public static class ExceptionUtils
{
    public static bool NotCancelledException(Exception exception)
    {
        return exception is not OperationCanceledException && (exception is not AggregateException aggregateException ||
                                                          !aggregateException.InnerExceptions.Any(o =>
                                                          {
                                                              return o is OperationCanceledException;
                                                          }));
    }
}