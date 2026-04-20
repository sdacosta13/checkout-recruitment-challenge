namespace PaymentGateway.Api.Services;

public interface ITimeService
{
    DateTime UtcNow();
}

public class DateTimeService : ITimeService
{
    public DateTime UtcNow()
        => DateTime.UtcNow;
}