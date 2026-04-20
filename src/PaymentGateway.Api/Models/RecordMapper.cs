using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Enums;
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

    public static PaymentResponse ToPaymentResponse(BankSimulatorResponse bankResponse, PaymentRecord record) =>
        new()
        {
            Id = record.Id,
            Status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = record.CardNumber[^4..],
            Amount = record.Amount,
            ExpiryMonth = record.ExpiryMonth,
            ExpiryYear = record.ExpiryYear,
        };

    private static Money ToAmount(MoneyDto recordAmount) =>
        new() { Amount = recordAmount.Amount, Currency = recordAmount.Currency };
}
