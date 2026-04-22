namespace PaymentGateway.Api.Models;

/// <summary>A monetary value expressed in a specific currency.</summary>
public record MoneyDto
{
    /// <summary>ISO 4217 currency code. Supported values: GBP, USD, EUR.</summary>
    /// <example>GBP</example>
    public required string Currency { get; init; }

    /// <summary>Amount in the smallest currency unit (e.g. pence for GBP, cents for USD).</summary>
    /// <example>100</example>
    public required int Amount { get; init; }
}