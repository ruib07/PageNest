using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class CartItemRepository : ICartItemRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<CartItem> CartItems => _context.CartItems;

    public CartItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItem>> GetCartItems()
    {
        return await CartItems.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsByUserId(Guid userId)
    {
        return await CartItems.AsNoTracking()
                              .Where(ci => ci.UserId == userId)
                              .ToListAsync();
    }

    public async Task<CartItem> GetCartItemById(Guid cartItemId)
    {
        return await CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId);
    }

    public async Task<CartItem> CreateCartItem(CartItem cartItem)
    {
        await CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        return cartItem;
    }

    public async Task UpdateCartItem(CartItem cartItem)
    {
        CartItems.Update(cartItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCartItem(Guid cartItemId)
    {
        var cartItem = await GetCartItemById(cartItemId);

        CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
    }
}
