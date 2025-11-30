// TradingApp.Shared/Helpers/RetryHelper.cs
namespace TradingApp.Shared.Helpers
{
    public static class RetryHelper
    {
        // Simple exponential backoff retry
        public static async Task<T> RetryOnExceptionAsync<T>(int maxAttempts, Func<Task<T>> action, Func<int, TimeSpan>? backoff = null)
        {
            backoff ??= attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt));
            Exception? lastEx = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    if (attempt == maxAttempts) throw;
                    await Task.Delay(backoff(attempt));
                }
            }
            throw lastEx!;
        }

        public static async Task RetryOnExceptionAsync(int maxAttempts, Func<Task> action, Func<int, TimeSpan>? backoff = null)
        {
            await RetryOnExceptionAsync<object?>(maxAttempts, async () => { await action(); return null; }, backoff);
        }
    }
}
