using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    // GET api/v1/payments
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
    {
        return Ok(await _paymentsService.GetPayments());
    }

    // GET api/v1/payments/order/{orderId}
    [Authorize]
    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByOrderId(Guid orderId)
    {
        return Ok(await _paymentsService.GetPaymentsByOrderId(orderId));
    }

    // GET api/v1/payments/{paymentId}
    [Authorize]
    [HttpGet("{paymentId}")]
    public async Task<IActionResult> GetPaymentById(Guid paymentId)
    {
        var result = await _paymentsService.GetPaymentById(paymentId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // POST api/v1/payments
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
    {
        var result = await _paymentsService.CreatePayment(payment);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(new ResponsesDTO.PaymentResponse(result.Message, result.Data.Id, result.Data.StripePaymentIntentId));
    }

    // PUT api/v1/payments/{paymentId}
    [Authorize]
    [HttpPut("{paymentId}")]
    public async Task<IActionResult> UpdatePayment(Guid paymentId, [FromBody] Payment updatePayment)
    {
        var result = await _paymentsService.UpdatePayment(paymentId, updatePayment);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Message);
    }

    // DELETE api/v1/payments/{paymentId}
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpDelete("{paymentId}")]
    public async Task<IActionResult> DeletePayment(Guid paymentId)
    {
        await _paymentsService.DeletePayment(paymentId);

        return NoContent();
    }
}
