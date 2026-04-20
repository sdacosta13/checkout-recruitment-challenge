using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services;

public interface IPaymentRepository
{
    void Add(PaymentRecord payment);
    bool TryGet(Guid id, out PaymentRecord? payment);
}

public class PaymentsRepository(ILogger<PaymentsRepository> logger) : IPaymentRepository
{
    private readonly List<PaymentRecord> _payments = new();
    
    public void Add(PaymentRecord payment)
    {
        _payments.Add(payment);
    }

    public bool TryGet(Guid id, out PaymentRecord? payment)
    {
        var matchingRecords = _payments.Where(p => p.Id == id).ToList();
        if (matchingRecords.Count == 1)
        {
            payment = matchingRecords[0];
            return true;
        }
            
        if (matchingRecords.Count > 1)
            logger.LogError("Multiple {RecordTyp} records found for {PaymentId}", nameof(PaymentRecord), id);
        
        payment = null;
        return false;
    }
}