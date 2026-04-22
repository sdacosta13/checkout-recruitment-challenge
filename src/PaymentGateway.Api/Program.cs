using FluentValidation;

using Microsoft.OpenApi;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Gateway API",
        Version = "v1",
        Description = "API for processing card payments through the acquiring bank."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddSingleton<IRetryPolicy, ExponentialBackoffRetryPolicy>();
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();
builder.Services.AddSingleton<ITimeService, DateTimeService>();
builder.Services.AddSingleton<IValidator<NewPaymentRequestDto>, NewPaymentRequestDtoValidator>();
builder.Services.AddHttpClient("BankSimulator", c =>
{
    c.BaseAddress = new Uri("http://localhost:8080");
});
builder.Services.AddTransient<IBankAccountClient, BankAccountClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
