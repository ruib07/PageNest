using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface ICartItemsService
{
    Task<IEnumerable<CartItem>> GetCartItems();
    Task<IEnumerable<CartItem>> GetCartItemsByUserId(Guid userId);
    Task<Result<CartItem>> GetCartItemById(Guid cartItemId);
    Task<Result<CartItem>> CreateCartItem(CartItem cartItem);
    Task<Result<CartItem>> UpdateCartItem(Guid cartItemId, CartItem updateCartItem);
    Task DeleteCartItem(Guid cartItemId);
}
