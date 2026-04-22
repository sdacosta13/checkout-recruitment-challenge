using System.Text.Json.Serialization;

using PaymentGateway.Api.Enums;
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
    Task<PaymentResponse?> AuthorizeAsync(PaymentRecord record, string cvv, CancellationToken ct = default);
}

public class BankAccountClient(IHttpClientFactory httpClientFactory) : IBankAccountClient
{
    private const string ClientName = "BankSimulator";

    public async Task<PaymentResponse?> AuthorizeAsync(PaymentRecord record, string cvv, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient(ClientName);

        var body = new BankSimulatorRequest(
            CardNumber: record.CardNumber,
            ExpiryDate: $"{record.ExpiryMonth:D2}/{record.ExpiryYear}",
            Currency:   record.Amount.Currency,
            Amount:     record.Amount.Amount,
            Cvv:        cvv
        );

        var response = await client.PostAsJsonAsync("/payments", body, ct);

        response.EnsureSuccessStatusCode();

        var bankResponse = await response.Content.ReadFromJsonAsync<BankSimulatorResponse>(cancellationToken: ct);

        return bankResponse is null 
            ? null 
            : ToPaymentResponse(bankResponse, record);
    }

    private static PaymentResponse ToPaymentResponse(BankSimulatorResponse bankResponse, PaymentRecord record) =>
        new()
        {
            Id = record.Id,
            Status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = record.CardNumber[^4..],
            Amount = record.Amount,
            ExpiryMonth = record.ExpiryMonth,
            ExpiryYear = record.ExpiryYear,
        };
}
