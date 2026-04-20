using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Models;

public static class RecordMapper
{
    public static PaymentRecord ToPaymentRecord(NewPaymentRequestDto record)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = record.CardNumber[^4..], 
            Amount = ToAmount(record.Amount),
            ExpiryMonth = record.ExpiryMonth,
            ExpiryYear = record.ExpiryYear,
        };

    }

    private static Money ToAmount(MoneyDto recordAmount)
    {
        return new Money()
        {
            Amount = recordAmount.Amount, 
            Currency = recordAmount.Currency,
        };
    }
}