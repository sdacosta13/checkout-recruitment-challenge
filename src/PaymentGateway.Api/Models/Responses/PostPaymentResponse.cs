namespace PaymentGateway.Api.Models.Responses;

/// <summary>Details of a payment that has been processed.</summary>
public record PaymentResponseDto
{
    /// <summary>Unique identifier assigned to this payment.</summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public required Guid Id { get; init; }

    /// <summary>Outcome of the authorisation attempt: <c>Authorized</c>, <c>Declined</c>.</summary>
    /// <example>Authorized</example>
    public required string Status { get; init; }

    /// <summary>Last four digits of the card number used for the payment.</summary>
    /// <example>8877</example>
    public required string CardNumberLastFour { get; init; }

    /// <summary>Card expiry month (1–12).</summary>
    /// <example>4</example>
    public required int ExpiryMonth { get; init; }

    /// <summary>Card expiry year (four-digit).</summary>
    /// <example>2025</example>
    public required int ExpiryYear { get; init; }

    /// <summary>The amount and currency that was charged.</summary>
    public required MoneyDto Amount { get; init; }
}
