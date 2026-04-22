using System.Collections.Concurrent;

using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services;

public interface IIdempotencyStore
{
    bool TryGet(string key, out PaymentResponse? response);
    void Set(string key, PaymentResponse response);
}

public class IdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, PaymentResponse> _store = new();

    public bool TryGet(string key, out PaymentResponse? response) =>
        _store.TryGetValue(key, out response);

    public void Set(string key, PaymentResponse response) =>
        _store[key] = response;
}
