using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/cartitems")]
[ApiController]
public class CartItemsController : ControllerBase
{
    private readonly ICartItemsService _cartItemsService;

    public CartItemsController(ICartItemsService cartItemsService)
    {
        _cartItemsService = cartItemsService;
    }

    // GET api/v1/cartitems
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
    {
        return Ok(await _cartItemsService.GetCartItems());
    }

    // GET api/v1/cartitems/user/{userId}
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItemsByUserId(Guid userId)
    {
        return Ok(await _cartItemsService.GetCartItemsByUserId(userId));
    }

    // GET api/v1/cartitems/{cartItemId}
    [Authorize]
    [HttpGet("{cartItemId}")]
    public async Task<IActionResult> GetCartItemById(Guid cartItemId)
    {
        var result = await _cartItemsService.GetCartItemById(cartItemId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // POST api/v1/cartitems
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCartItem([FromBody] CartItem cartItem)
    {
        var result = await _cartItemsService.CreateCartItem(cartItem);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        var response = new ResponsesDTO.Creation(result.Message, result.Data.Id);
        
        return CreatedAtAction(nameof(GetCartItemById), new { cartItemId = result.Data.Id }, response);
    }

    // PUT api/v1/cartitems/{cartItemId}
    [Authorize]
    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] CartItem updateCartItem)
    {
        var result = await _cartItemsService.UpdateCartItem(cartItemId, updateCartItem);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Message);
    }

    // DELETE api/v1/cartitems/{cartItemId}
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> DeleteCartItem(Guid cartItemId)
    {
        await _cartItemsService.DeleteCartItem(cartItemId);

        return NoContent();
    }
}
