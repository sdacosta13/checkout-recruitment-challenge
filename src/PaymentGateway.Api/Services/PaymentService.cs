using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<PaymentResponse?> AuthorizeAsync(NewPaymentRequestDto request, CancellationToken ct = default);
}

public class PaymentService(IBankAccountClient bankClient, IRetryPolicy retryPolicy) : IPaymentService
{
    public Task<PaymentResponse?> AuthorizeAsync(NewPaymentRequestDto request, CancellationToken ct = default)
    {
        var record = RecordMapper.ToPaymentRecord(request);
        return retryPolicy.ExecuteAsync(() => bankClient.AuthorizeAsync(record, ct), ct);
    }
}
