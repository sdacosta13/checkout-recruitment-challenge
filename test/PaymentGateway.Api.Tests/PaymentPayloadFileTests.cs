using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Entities;

using Xunit.Abstractions;
using Xunit.Extensions.Logging;

namespace PaymentGateway.Api.Tests;

file record PayloadTestCase(string Description, JsonElement Payload, int ExpectedStatus);

file class AlwaysAuthorizedBankClient : IBankAccountClient
{
    public Task<PaymentResponse?> AuthorizeAsync(PaymentRecord record, CancellationToken ct = default) =>
        Task.FromResult<PaymentResponse?>(new PaymentResponse
        {
            Id = record.Id,
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = record.CardNumber[^4..],
            Amount = record.Amount,
            ExpiryMonth = record.ExpiryMonth,
            ExpiryYear = record.ExpiryYear,
        });
}

public class PaymentPayloadFileTests(ITestOutputHelper output)
{
    private static IEnumerable<object[]> LoadTestCases(string filename)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", filename);
        return File.ReadLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<PayloadTestCase>(line,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!)
            .Select(tc => new object[] { tc.Description, tc.Payload.ToString(), tc.ExpectedStatus });
    }

    public static IEnumerable<object[]> TestCases => LoadTestCases("payment_payloads.jsonl");
    public static IEnumerable<object[]> MissingFieldCases => LoadTestCases("missing_fields_payloads.jsonl");

    private WebApplicationFactory<PaymentsController> CreateFactory() =>
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); logging.AddXunit(output); });
            builder.ConfigureServices(services =>
                services.AddSingleton<IBankAccountClient, AlwaysAuthorizedBankClient>());
        });

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task PostPayment_ReturnsExpectedStatus(string description, string payloadJson, int expectedStatus)
    {
        output.WriteLine($"Case: {description}");
        var client = CreateFactory().CreateClient();

        using var content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/payments", content);

        Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(MissingFieldCases))]
    public async Task PostPayment_MissingField_ReturnsBadRequest(string description, string payloadJson, int expectedStatus)
    {
        output.WriteLine($"Case: {description}");
        var client = CreateFactory().CreateClient();

        using var content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/payments", content);

        Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
    }
}
