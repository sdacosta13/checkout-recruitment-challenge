using System.Text.Json.Serialization;

using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Clients;

public record BankSimulatorRequest(
    [property: JsonPropertyName("card_number")] string CardNumber,
    [property: JsonPropertyName("expiry_date")] string ExpiryDate,
    [property: JsonPropertyName("currency")]    string Currency,
    [property: JsonPropertyName("amount")]      int    Amount,
    [property: JsonPropertyName("cvv")]         string Cvv
);

public record BankSimulatorResponse(
    [property: JsonPropertyName("authorized")]         bool   Authorized,
    [property: JsonPropertyName("authorization_code")] string AuthorizationCode
);

public interface IBankAccountClient
{
    Task<BankSimulatorResponse?> AuthorizeAsync(PaymentRecord record, CancellationToken ct = default);
}

public class BankAccountClient(IHttpClientFactory httpClientFactory) : IBankAccountClient
{
    private const string ClientName = "BankSimulator";

    public async Task<BankSimulatorResponse?> AuthorizeAsync(PaymentRecord record, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient(ClientName);

        var body = new BankSimulatorRequest(
            CardNumber: record.CardNumber,
            ExpiryDate: $"{record.ExpiryMonth:D2}/{record.ExpiryYear}",
            Currency:   record.Amount.Currency,
            Amount:     record.Amount.Amount,
            Cvv:        record.Cvv
        );

        var response = await client.PostAsJsonAsync("/payments", body, ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BankSimulatorResponse>(cancellationToken: ct);
    }
}
