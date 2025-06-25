using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface ICartItemRepository
{
    Task<IEnumerable<CartItem>> GetCartItems();
    Task<IEnumerable<CartItem>> GetCartItemsByUserId(Guid userId);
    Task<CartItem> GetCartItemById(Guid cartItemId);
    Task<CartItem> CreateCartItem(CartItem cartItem);
    Task UpdateCartItem(CartItem cartItem);
    Task DeleteCartItem(Guid cartItemId);
}
