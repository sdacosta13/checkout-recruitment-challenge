using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

using Xunit.Abstractions;

namespace PaymentGateway.Api.Tests;

file class NoRetryPolicy : IRetryPolicy
{
    public async Task<(T? response, int attempts)> ExecuteAsync<T>(Func<Task<T?>> operation, CancellationToken ct = default)
    {
        try
        {
            return (await operation(), 1);
        }
        catch (HttpRequestException)
        {
            return default;
        }
    }
}

public class PaymentsControllerTests(ITestOutputHelper output)
{
    private readonly PaymentResponse _authorizedPayment = new()
    {
        Id = Guid.NewGuid(),
        Status = PaymentStatus.Authorized,
        CardNumberLastFour = "7455",
        ExpiryYear = 2026,
        ExpiryMonth = 10,
        Amount = new Money { Amount = 1000, Currency = "GBP" },
    };

    private WebApplicationFactory<PaymentsController> CreateFactory() =>
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); logging.AddXunit(output); });
            builder.ConfigureServices(services =>
                services.AddSingleton<ITimeService>(new FrozenTimeService(FrozenTimeService.Default)));
        });

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        var repository = factory.Services.GetRequiredService<IPaymentRepository>();
        repository.Add(_authorizedPayment);

        // Act
        var response = await client.GetAsync($"/api/v1/payments/{_authorizedPayment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(_authorizedPayment.Id, paymentResponse.Id);
        Assert.Equal(_authorizedPayment.Status.ToString(), paymentResponse.Status);
        Assert.Equal(_authorizedPayment.CardNumberLastFour, paymentResponse.CardNumberLastFour);
        Assert.Equal(_authorizedPayment.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(_authorizedPayment.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(_authorizedPayment.Amount.Amount, paymentResponse.Amount.Amount);
        Assert.Equal(_authorizedPayment.Amount.Currency, paymentResponse.Amount.Currency);
    }

    [Fact]
    public async Task Returns502WhenBankIsDown()
    {
        // Arrange
        var factory = new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); logging.AddXunit(output); });
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient("BankSimulator", c => c.BaseAddress = new Uri("http://localhost:1"));
                services.AddSingleton<IRetryPolicy, NoRetryPolicy>();
                services.AddSingleton<ITimeService>(new FrozenTimeService(FrozenTimeService.Default));
            });
        });

        var request = new
        {
            cardNumber = "2222405343248877",
            expiryMonth = 4,
            expiryYear = 2027,
            cvv = "123",
            amount = new { currency = "GBP", amount = 100 }
        };

        // Act
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var client = CreateFactory().CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
