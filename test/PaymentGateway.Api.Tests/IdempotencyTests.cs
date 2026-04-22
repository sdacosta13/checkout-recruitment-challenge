using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using Xunit.Abstractions;

namespace PaymentGateway.Api.Tests;

[Collection(BankSimulatorCollection.Name)]
public class IdempotencyTests(BankSimulatorFixture bankSimulator, ITestOutputHelper output)
{
    private static readonly object ValidPayload = new
    {
        cardNumber = "2222405343248877",
        expiryMonth = 4,
        expiryYear = 2030,
        cvv = "123",
        amount = new { currency = "GBP", amount = 100 }
    };

    private WebApplicationFactory<PaymentsController> CreateFactory() =>
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); logging.AddXunit(output); });
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient("BankSimulator", c => c.BaseAddress = bankSimulator.BaseAddress);
                services.AddSingleton<ITimeService>(new FrozenTimeService(FrozenTimeService.Default));
            });
        });

    [Fact]
    public async Task SameKey_ReturnsSamePaymentId()
    {
        // Arrange
        var client = CreateFactory().CreateClient();
        var key = Guid.NewGuid().ToString();

        // Act
        var first = await PostAsync(client, ValidPayload, key);
        var second = await PostAsync(client, ValidPayload, key);

        // Assert
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstPayment = await first.Content.ReadFromJsonAsync<PaymentResponseDto>();
        var secondPayment = await second.Content.ReadFromJsonAsync<PaymentResponseDto>();

        Assert.Equal(firstPayment!.Id, secondPayment!.Id);
    }

    [Fact]
    public async Task DifferentKeys_ReturnDifferentPaymentIds()
    {
        // Arrange
        var client = CreateFactory().CreateClient();

        // Act
        var first = await PostAsync(client, ValidPayload, Guid.NewGuid().ToString());
        var second = await PostAsync(client, ValidPayload, Guid.NewGuid().ToString());

        // Assert
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstPayment = await first.Content.ReadFromJsonAsync<PaymentResponseDto>();
        var secondPayment = await second.Content.ReadFromJsonAsync<PaymentResponseDto>();

        Assert.NotEqual(firstPayment!.Id, secondPayment!.Id);
    }

    [Fact]
    public async Task NoKey_ProcessesNormally()
    {
        // Arrange
        var client = CreateFactory().CreateClient();

        // Act
        var response = await PostAsync(client, ValidPayload, idempotencyKey: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
        Assert.NotNull(payment);
        Assert.NotEqual(Guid.Empty, payment.Id);
    }

    [Fact]
    public async Task InvalidPayload_WithKey_IsNotCached_AllowsRetryWithValidPayload()
    {
        // Arrange
        var client = CreateFactory().CreateClient();
        var key = Guid.NewGuid().ToString();
        var invalidPayload = new
        {
            cardNumber = "invalid",
            expiryMonth = 4,
            expiryYear = 2030,
            cvv = "123",
            amount = new { currency = "GBP", amount = 100 }
        };

        // Act
        var badResponse = await PostAsync(client, invalidPayload, key);
        var goodResponse = await PostAsync(client, ValidPayload, key);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, badResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, goodResponse.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, object payload, string? idempotencyKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(payload)
        };
        if (idempotencyKey is not null)
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        return await client.SendAsync(request);
    }
}
