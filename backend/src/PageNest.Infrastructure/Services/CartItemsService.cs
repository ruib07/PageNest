using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class CartItemsService : ICartItemsService
{
    private readonly ICartItemRepository _cartItemRepository;

    public CartItemsService(ICartItemRepository cartItemRepository)
    {
        _cartItemRepository = cartItemRepository;
    }

    public async Task<IEnumerable<CartItem>> GetCartItems()
    {
        return await _cartItemRepository.GetCartItems();
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsByUserId(Guid userId)
    {
        return await _cartItemRepository.GetCartItemsByUserId(userId);
    }

    public async Task<Result<CartItem>> GetCartItemById(Guid cartItemId)
    {
        var cartItem = await _cartItemRepository.GetCartItemById(cartItemId);

        if (cartItem == null)
            return Result<CartItem>.Fail("Cart item not found.", 404);

        return Result<CartItem>.Success(cartItem);
    }

    public async Task<Result<CartItem>> CreateCartItem(CartItem cartItem)
    {
        var validation = await ValidateCartItem(cartItem, checkForDuplicate: true);

        if (!validation.IsSuccess)
            return Result<CartItem>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var createdCartItem = await _cartItemRepository.CreateCartItem(cartItem);

        return Result<CartItem>.Success(createdCartItem, "Cart item created successfully.");
    }

    public async Task<Result<CartItem>> UpdateCartItem(Guid cartItemId, CartItem updateCartItem)
    {
        var currentCartItem = await _cartItemRepository.GetCartItemById(cartItemId);

        var validation = await ValidateCartItem(updateCartItem);

        if (!validation.IsSuccess)
            return Result<CartItem>.Fail(validation.Error.Message, validation.Error.StatusCode);

        currentCartItem.Quantity = updateCartItem.Quantity;

        await _cartItemRepository.UpdateCartItem(currentCartItem);

        return Result<CartItem>.Success(currentCartItem, "Cart item updated successfully.");
    }

    public async Task DeleteCartItem(Guid cartItemId)
    {
        await _cartItemRepository.DeleteCartItem(cartItemId);
    }

    #region Private Methods

    private async Task<Result<bool>> ValidateCartItem(CartItem cartItem, bool checkForDuplicate = false)
    {
        if (cartItem == null)
            return Result<bool>.Fail("Cart item cannot be null.", 400);

        if (cartItem.Quantity <= 0)
            return Result<bool>.Fail("Quantity must be greater than zero.", 400);

        if (checkForDuplicate)
        {
            var userItems = await _cartItemRepository.GetCartItemsByUserId(cartItem.UserId);
            var exists = userItems.Any(ci => ci.BookId == cartItem.BookId);

            if (exists) return Result<bool>.Fail("Book already exists in the cart.", 409);
        }

        return Result<bool>.Success(true);
    }

    #endregion
}
