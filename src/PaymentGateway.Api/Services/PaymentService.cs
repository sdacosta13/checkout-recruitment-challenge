using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<PaymentResponse?> AuthorizeAsync(NewPaymentRequestDto request, CancellationToken ct = default);
}

public class PaymentService(IBankAccountClient bankClient) : IPaymentService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);

    public async Task<PaymentResponse?> AuthorizeAsync(NewPaymentRequestDto request, CancellationToken ct = default)
    {
        var record = RecordMapper.ToPaymentRecord(request);

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            try
            {
                var bankResponse = await bankClient.AuthorizeAsync(record, ct);
                return bankResponse is null ? null : RecordMapper.ToPaymentResponse(bankResponse, record);
            }
            catch (HttpRequestException) when (attempt < MaxAttempts - 1)
            {
                var delay = BaseDelay * Math.Pow(2, attempt);
                await Task.Delay(delay, ct);
            }
        }

        return null;
    }
}
