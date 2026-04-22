namespace PaymentGateway.Api.Models.Entities;

public class PaymentRecord
{
    public required Guid Id { get; init; }
    public required string CardNumber { get; init; }
    public required int ExpiryMonth { get; init; }
    public required int ExpiryYear { get; init; }
    public required Money Amount { get; init; }
}

public record Money
{
    public required string Currency { get; init; }
    public required int Amount { get; init; }

    public static IReadOnlyList<string> SupportedCurrencies =>
    [
        "GBP",
        "USD",
        "EUR"
    ];
}