using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services;

public interface IPaymentRepository
{
    void Add(PaymentResponse payment);
    bool TryGet(Guid id, out PaymentResponse? payment);
}

public class PaymentsRepository(ILogger<PaymentsRepository> logger) : IPaymentRepository
{
    private readonly List<PaymentResponse> _payments = new();

    public void Add(PaymentResponse payment)
    {
        _payments.Add(payment);
    }

    public bool TryGet(Guid id, out PaymentResponse? payment)
    {
        var matchingRecords = _payments.Where(p => p.Id == id).ToList();
        if (matchingRecords.Count == 1)
        {
            payment = matchingRecords[0];
            return true;
        }

        if (matchingRecords.Count > 1)
            logger.LogError("Multiple {RecordType} records found for {PaymentId}", nameof(PaymentResponse), id);

        payment = null;
        return false;
    }
}
