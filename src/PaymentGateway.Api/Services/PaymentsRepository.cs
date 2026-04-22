using System.Collections.Concurrent;

using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services;

public interface IPaymentRepository
{
    void Add(PaymentResponse payment);
    bool TryGet(Guid id, out PaymentResponse? payment);
}

public class PaymentsRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<Guid, PaymentResponse> _payments = new();

    public void Add(PaymentResponse payment) => _payments[payment.Id] = payment;

    public bool TryGet(Guid id, out PaymentResponse? payment) =>
        _payments.TryGetValue(id, out payment);
}
