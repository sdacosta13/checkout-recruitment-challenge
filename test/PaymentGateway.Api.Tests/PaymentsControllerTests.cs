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

public class PaymentsControllerTests(ITestOutputHelper output)
{
    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PaymentRecord
        {
            ExpiryYear = 2026,
            ExpiryMonth = 10,
            Amount = new Money()
            {
                Amount = 1000,
                Currency = "GBP"
            },
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "1478569874587455",
        };

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var application = webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); logging.AddXunit(output); });
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton<IPaymentRepository, PaymentsRepository>());
        });
        var client = application.CreateClient();
        var repository = application.Services.GetRequiredService<IPaymentRepository>();
        repository.Add(payment);
        
        // Act
        var response = await client.GetAsync($"/api/payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => { logging.ClearProviders(); logging.AddXunit(output); }))
            .CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}