using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentRepository paymentsRepository,
    IPaymentService paymentService,
    IValidator<NewPaymentRequestDto> paymentRequestValidator) : Controller
{
    [HttpGet("{id:guid}")]
    public ActionResult<PaymentResponseDto> GetPayment([FromRoute] Guid id)
    {
        var found = paymentsRepository.TryGet(id, out var paymentResponse);
        if (!found)
            return NotFound();

        return Ok(DtoMapper.ToDto(paymentResponse));
    }

    [HttpPost]
    public async Task<ActionResult> PostPayment([FromBody] NewPaymentRequestDto dto)
    {
        var result = await paymentRequestValidator.ValidateAsync(dto);
        if (!result.IsValid)
            return BadRequest(result.Errors);

        var paymentResponse = await paymentService.AuthorizeAsync(dto);
        if (paymentResponse is null)
            return StatusCode(502);

        paymentsRepository.Add(paymentResponse);

        return Ok(DtoMapper.ToDto(paymentResponse));
    }
}
