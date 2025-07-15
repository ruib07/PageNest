using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/orderitems")]
public class OrderItemsController : ControllerBase
{
    private readonly IOrderItemsService _orderItemsService;

    public OrderItemsController(IOrderItemsService orderItemsService)
    {
        _orderItemsService = orderItemsService;
    }

    // GET api/v1/orderitems
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
    {
        return Ok(await _orderItemsService.GetOrderItems());
    }

    // GET api/v1/orderitems/order/{orderId}
    [Authorize]
    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItemsByOrderId(Guid orderId)
    {
        return Ok(await _orderItemsService.GetOrderItemsByOrderId(orderId));
    }

    // GET api/v1/orderitems/book/{bookId}
    [Authorize]
    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItemsByBookId(Guid bookId)
    {
        return Ok(await _orderItemsService.GetOrderItemsByBookId(bookId));
    }

    // GET api/v1/orderitems/{orderItemId}
    [Authorize]
    [HttpGet("{orderItemId}")]
    public async Task<IActionResult> GetOrderItemById(Guid orderItemId)
    {
        var result = await _orderItemsService.GetOrderItemById(orderItemId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }
}
