using FluentValidation;

using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Models.Requests;

public record NewPaymentRequestDto
{
    public required string CardNumber { get; init; }
    public required int ExpiryMonth { get; init; }
    public required int ExpiryYear { get; init; }
    public required string Cvv { get; init; }   
    public required MoneyDto Amount { get; init; }
}

public class NewPaymentRequestDtoValidator : AbstractValidator<NewPaymentRequestDto>
{
    private readonly ITimeService _timeService;

    public NewPaymentRequestDtoValidator(ITimeService timeService)
    {
        _timeService = timeService;
        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .Matches("^\\d{14,19}$")
            .WithMessage("Card number must be between 14 and 19 digits long");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.ExpiryYear)
            .GreaterThan(0);
        
        RuleFor(x => x)
            .Must(HaveExpiryInTheFuture)
            .WithMessage("Expiry date must be in the future");
        
        RuleFor(x => x.Cvv)
            .Matches("^\\d{3,4}$")
            .WithMessage("Card number must be between 14 and 19 digits long");

        RuleFor(x => x.Amount)
            .Must(BeNonNegative)
            .WithMessage("Amount must be non-negative")
            .Must(BeValidCurrency)
            .WithMessage($"Must be a valid currency. Supported currencies: [{string.Join(", ",Money.SupportedCurrencies)}]");
    }

    private bool BeValidCurrency(MoneyDto money) 
        => Money.SupportedCurrencies.Contains(money.Currency);

    private bool BeNonNegative(MoneyDto money)
    {
        return money.Amount >= 0; // Could be a test payment of £0
    }

    private bool HaveExpiryInTheFuture(NewPaymentRequestDto request)
    {
        var now = _timeService.UtcNow();
        var expiry = new DateTime(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1);
        return expiry > now;
    }
}