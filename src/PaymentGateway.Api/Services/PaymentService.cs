using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    
}

public class PaymentService() : IPaymentService
{
    public PaymentStatus ValidatePayment(PaymentRecord record)
    {
        return PaymentStatus.Authorized;
    }
}