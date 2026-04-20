using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Models;

public static class RecordMapper
{
    public static PaymentRecord ToPaymentRecord(NewPaymentRequestDto request) =>
        new()
        {
            Id = Guid.NewGuid(),
            CardNumber = request.CardNumber,
            Cvv = request.Cvv,
            Amount = ToAmount(request.Amount),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
        };

    private static Money ToAmount(MoneyDto recordAmount) =>
        new() { Amount = recordAmount.Amount, Currency = recordAmount.Currency };
}
