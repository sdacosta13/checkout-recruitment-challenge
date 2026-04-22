using FluentValidation;

using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Models.Requests;

/// <summary>Payload for submitting a new card payment.</summary>
public record NewPaymentRequestDto
{
    /// <summary>Full card number (14–19 digits, no spaces or dashes).</summary>
    /// <example>2222405343248877</example>
    public required string CardNumber { get; init; }

    /// <summary>Card expiry month (1–12).</summary>
    /// <example>4</example>
    public required int ExpiryMonth { get; init; }

    /// <summary>Card expiry year (four-digit, must be in the future).</summary>
    /// <example>2025</example>
    public required int ExpiryYear { get; init; }

    /// <summary>Card security code (3–4 digits).</summary>
    /// <example>123</example>
    public required string Cvv { get; init; }

    /// <summary>The amount and currency to charge.</summary>
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
            .WithMessage("The CVV must be between 3 and 4 digits long");

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
        if (request.ExpiryMonth is < 1 or > 12 || request.ExpiryYear <= 0)
            return false;
        var now = _timeService.UtcNow();
        var expiry = new DateTime(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1);
        return expiry > now;
    }
}