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
    IValidator<NewPaymentRequestDto> paymentRequestValidator) : Controller
{
    [HttpGet("{id:guid}")]
    public ActionResult<PaymentResponseDto> GetPayment([FromRoute] Guid id)
    {
        var found = paymentsRepository.TryGet(id, out var paymentRecord);
        if (!found)
            return NotFound();
        
        var response = DtoMapper.ToDo(paymentRecord);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> PostPayment([FromBody] NewPaymentRequestDto dto)
    {
        var result = await paymentRequestValidator.ValidateAsync(dto);
        if(!result.IsValid)
            return BadRequest(result.Errors);
        
        paymentsRepository.Add(RecordMapper.ToPaymentRecord(dto));
        
        return Ok(NewPaymentResponseDto.Accepted());
    }
}