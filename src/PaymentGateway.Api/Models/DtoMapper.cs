using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models;

public static class DtoMapper
{
    public static PaymentResponseDto ToDto(PaymentResponse response)
    {
        return new()
        {
            Id = response.Id,
            Status = response.Status.ToString(),
            CardNumberLastFour = response.CardNumberLastFour,
            ExpiryMonth = response.ExpiryMonth,
            ExpiryYear = response.ExpiryYear,
            Amount = new MoneyDto { Amount = response.Amount.Amount, Currency = response.Amount.Currency },
        };
    }
}
