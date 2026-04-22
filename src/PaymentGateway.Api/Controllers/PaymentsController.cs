using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

/// <summary>Manages payment authorisation and retrieval.</summary>
[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
public class PaymentsController(IPaymentRepository paymentsRepository,
    IPaymentService paymentService,
    IValidator<NewPaymentRequestDto> paymentRequestValidator) : Controller
{
    /// <summary>Retrieves a previously authorised payment by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>The payment details.</returns>
    /// <response code="200">Payment found and returned.</response>
    /// <response code="404">No payment exists with the given identifier.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaymentResponseDto> GetPayment([FromRoute] Guid id)
    {
        var found = paymentsRepository.TryGet(id, out var paymentResponse);
        if (!found)
            return NotFound();

        return Ok(DtoMapper.ToDto(paymentResponse));
    }

    /// <summary>Submits a new card payment for authorisation through the acquiring bank.</summary>
    /// <param name="dto">The payment details including card information and amount.</param>
    /// <returns>The authorisation result with a generated payment identifier.</returns>
    /// <response code="200">Payment processed (authorised or declined). Check the <c>Status</c> field.</response>
    /// <response code="400">The request payload failed validation.</response>
    /// <response code="502">The acquiring bank could not be reached.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult> PostPayment([FromBody] NewPaymentRequestDto dto)
    {
        var result = await paymentRequestValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem();
        }

        var paymentResponse = await paymentService.AuthorizeAsync(dto);
        if (paymentResponse is null)
            return Problem(statusCode: 502, title: "Bad Gateway", detail: "The acquiring bank could not be reached.");

        paymentsRepository.Add(paymentResponse);

        return Ok(DtoMapper.ToDto(paymentResponse));
    }
}
