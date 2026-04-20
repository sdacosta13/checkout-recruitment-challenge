using FluentValidation;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();
builder.Services.AddSingleton<ITimeService, DateTimeService>();
builder.Services.AddSingleton<IValidator<NewPaymentRequestDto>, NewPaymentRequestDtoValidator>();
builder.Services.AddHttpClient();

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
