namespace B1ServiceLayer.Helpers;

public static class RetryHelper
{
    public static void Retry(Action action, int retryTimes = 1, Action? recoverAction = null)
    {
        for (int i = 0; i < retryTimes; i++)
        {
            try
            {
                action();
                break;
            }
            catch
            {
                recoverAction?.Invoke();

                if (i == retryTimes - 1)
                    throw;
            }
        }
    }

    public static async ValueTask Retry(Func<ValueTask> func, int retryTimes = 1)
    {
        for (int i = 0; i < retryTimes; i++)
        {
            try
            {
                await func();
                break;
            }
            catch
            {
                if (i == retryTimes - 1)
                    throw;
            }
        }
    }

    public static async ValueTask<T?> Retry<T>(Func<ValueTask> func, int retryTimes = 1)
    {
        for (int i = 0; i < retryTimes; i++)
        {
            try
            {
                await func();
                break;
            }
            catch
            {
                if (i == retryTimes - 1)
                    throw;
            }
        }

        return default;
    }
}
