namespace PaymentGateway.Api.Models.Responses;

public record PaymentResponseDto
{
    public required Guid Id { get; set; }
    public required string Status { get; set; }
    public required string CardNumberLastFour { get; set; }
    public required int ExpiryMonth { get; set; }
    public required int ExpiryYear { get; set; }
    public required MoneyDto Amount { get; set; }
}
