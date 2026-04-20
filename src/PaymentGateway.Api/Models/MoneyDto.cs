namespace PaymentGateway.Api.Models;

public record MoneyDto
{
    public required string Currency { get; init; }
    public required int Amount { get; init; }
    
}