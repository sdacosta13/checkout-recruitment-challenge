using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models;

public static class DtoMapper
{
    public static PaymentResponseDto? ToDo(PaymentRecord? record)
    {
        if (record == null)
            return null;
        
        return new()
        {
            Amount = ToMoneyDto(record.Amount),
            CardNumberLastFour = record.CardNumberLastFour,
            ExpiryYear = record.ExpiryYear,
            ExpiryMonth = record.ExpiryMonth,
            Id = record.Id,
            Status = ToPaymentStatusDto(record.Status),
        };
    }

    private static string ToPaymentStatusDto(PaymentStatus status)
    {
        return status.ToString();
    }

    private static MoneyDto ToMoneyDto(Money money)
        => new()
        {
            Amount = money.Amount, 
            Currency = money.Currency,
        };
}