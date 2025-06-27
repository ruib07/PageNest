using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    // GET api/v1/orders
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return Ok(await _ordersService.GetOrders());
    }

    // GET api/v1/orders/user/{userId}
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUserId(Guid userId)
    {
        return Ok(await _ordersService.GetOrdersByUserId(userId));
    }

    // GET api/v1/orders/{orderId}
    [Authorize]
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        var result = await _ordersService.GetOrderById(orderId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // POST api/v1/orders
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        var result = await _ordersService.CreateOrder(order);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        var response = new ResponsesDTO.Creation(result.Message, result.Data.Id);

        return CreatedAtAction(nameof(GetOrderById), new { orderId = result.Data.Id }, response);
    }

    // PUT api/v1/orders/{orderId}
    [Authorize]
    [HttpPut("{orderId}")]
    public async Task<IActionResult> UpdateOrder(Guid orderId, [FromBody] Order updateOrder)
    {
        var result = await _ordersService.UpdateOrder(orderId, updateOrder);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Message);
    }

    // DELETE api/v1/orders/{orderId}
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpDelete("{orderId}")]
    public async Task<IActionResult> DeleteOrder(Guid orderId)
    {
        await _ordersService.DeleteOrder(orderId);

        return NoContent();
    }
}
