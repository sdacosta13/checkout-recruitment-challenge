namespace PaymentGateway.Api.Models.Responses;

public class NewPaymentResponseDto
{
    public static NewPaymentResponseDto Accepted()
    {
        return new NewPaymentResponseDto() { Status = "Accepted", };
    }

    public static NewPaymentResponseDto Declined()
    {
        return new NewPaymentResponseDto() { Status = "Declined", };
    }
    
    public required string Status { get; init; }
}