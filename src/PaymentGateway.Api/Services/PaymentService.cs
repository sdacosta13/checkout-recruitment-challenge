using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<PaymentResponse?> AuthorizeAsync(NewPaymentRequestDto request, CancellationToken ct = default);
}

public class PaymentService(ILogger<PaymentService> paymentService, IBankAccountClient bankClient, IRetryPolicy retryPolicy) : IPaymentService
{
    public async Task<PaymentResponse?> AuthorizeAsync(NewPaymentRequestDto request, CancellationToken ct = default)
    {
        var record = RecordMapper.ToPaymentRecord(request);
        var (response, attemptCount) = await retryPolicy.ExecuteAsync(() => bankClient.AuthorizeAsync(record, request.Cvv, ct), ct);
        
        if(response is null)
            paymentService.LogWarning("Could not authorize payment record as we did not receive a response from the backend after {attempts}", attemptCount);
        else
            paymentService.LogInformation("The payment record of id {id} was {status} after {attemptCount} attempt(s)", response.Id, response.Status, attemptCount);
        
        return response;
    }
}
