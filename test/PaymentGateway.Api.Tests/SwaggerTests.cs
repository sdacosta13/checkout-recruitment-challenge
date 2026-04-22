using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class SwaggerTests
{
    private readonly WebApplicationFactory<PaymentsController> _factory =
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddSingleton<ITimeService>(new FrozenTimeService(FrozenTimeService.Default))));

    [Fact]
    public async Task SwaggerJson_ReturnsOk()
    {
        var response = await _factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJson_IsValidJson()
    {
        var response = await _factory.CreateClient().GetAsync("/swagger/v1/swagger.json");
        var json = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.NotNull(json);
    }

    [Fact]
    public async Task SwaggerJson_ContainsExpectedEndpoints()
    {
        var response = await _factory.CreateClient().GetAsync("/swagger/v1/swagger.json");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        var paths = doc!.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/Payments", out _), "Missing POST /api/v1/Payments");
        Assert.True(paths.TryGetProperty("/api/v1/Payments/{id}", out _), "Missing GET /api/v1/Payments/{id}");
    }
}
