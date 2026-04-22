using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

internal sealed class FrozenTimeService(DateTime frozenUtc) : ITimeService
{
    public static readonly DateTime Default = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public DateTime UtcNow() => frozenUtc;
}
