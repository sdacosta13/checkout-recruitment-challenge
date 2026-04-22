namespace PaymentGateway.Api.Services;

public interface IRetryPolicy
{
    Task<(T? response, int attempts)> ExecuteAsync<T>(Func<Task<T?>> operation, CancellationToken ct = default);
}

public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);

    public async Task<(T? response, int attempts)> ExecuteAsync<T>(Func<Task<T?>> operation, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            try
            {
                return (await operation(), attempt + 1);
            }
            catch (HttpRequestException)
            {
                // Always catch so the final attempt falls through to return null rather than throwing
                if (attempt < MaxAttempts - 1)
                    await Task.Delay(BaseDelay * Math.Pow(2, attempt), ct);
            }
        }

        return default;
    }
}
