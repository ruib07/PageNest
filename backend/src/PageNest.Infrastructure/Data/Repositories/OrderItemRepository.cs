using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<OrderItem> OrderItems => _context.OrderItems;

    public OrderItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItems()
    {
        return await OrderItems.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(Guid orderId)
    {
        return await OrderItems.AsNoTracking().Where(oi => oi.OrderId == orderId).ToListAsync(); 
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByBookId(Guid bookId)
    {
        return await OrderItems.AsNoTracking().Where(oi => oi.BookId == bookId).ToListAsync();
    }

    public async Task<OrderItem> GetOrderItemById(Guid orderItemId)
    {
        return await OrderItems.FirstOrDefaultAsync(oi => oi.Id == orderItemId);
    }
}
