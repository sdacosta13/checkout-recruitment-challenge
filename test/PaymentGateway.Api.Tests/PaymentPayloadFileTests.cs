using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Responses;

using Xunit.Abstractions;
using Xunit.Extensions.Logging;

namespace PaymentGateway.Api.Tests;

file record PayloadTestCase(string Description, JsonElement Payload, int ExpectedStatus);

[Collection(BankSimulatorCollection.Name)]
public class PaymentPayloadFileTests(BankSimulatorFixture bankSimulator, ITestOutputHelper output)
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
                services.AddHttpClient("BankSimulator", c => c.BaseAddress = bankSimulator.BaseAddress));
        });

    private static async Task AssertResponseBodyAsync(HttpResponseMessage response, int expectedStatus)
    {
        switch (expectedStatus)
        {
            case 200:
                var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
                Assert.NotNull(payment);
                Assert.NotEqual(Guid.Empty, payment.Id);
                Assert.True(payment.Status is "Authorized" or "Declined");
                Assert.Equal(4, payment.CardNumberLastFour.Length);
                Assert.InRange(payment.ExpiryMonth, 1, 12);
                Assert.True(payment.ExpiryYear > 0);
                Assert.NotNull(payment.Amount);
                Assert.True(payment.Amount.Amount >= 0);
                Assert.NotEmpty(payment.Amount.Currency);
                break;

            case 400:
                var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                Assert.NotNull(validationProblem);
                Assert.Equal(400, validationProblem.Status);
                Assert.NotEmpty(validationProblem.Errors);
                break;

            case 502:
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                Assert.NotNull(problem);
                Assert.Equal(502, problem.Status);
                Assert.NotEmpty(problem.Title!);
                break;
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task PostPayment_ReturnsExpectedStatus(string description, string payloadJson, int expectedStatus)
    {
        output.WriteLine($"Case: {description}");
        var client = CreateFactory().CreateClient();

        using var content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/payments", content);

        Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
        await AssertResponseBodyAsync(response, expectedStatus);
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
        await AssertResponseBodyAsync(response, expectedStatus);
    }
}
