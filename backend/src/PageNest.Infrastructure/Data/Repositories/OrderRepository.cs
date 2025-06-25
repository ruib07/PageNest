using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<Order> Orders => _context.Orders;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetOrders()
    {
        return await Orders.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserId(Guid userId)
    {
        return await Orders.AsNoTracking().Where(o => o.UserId == userId).ToListAsync();
    }

    public async Task<Order> GetOrderById(Guid orderId)
    {
        return await Orders.FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order> CreateOrder(Order order)
    {
        await Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task UpdateOrder(Order order)
    {
        Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrder(Guid orderId)
    {
        var order = await GetOrderById(orderId);

        Orders.Remove(order);
        await _context.SaveChangesAsync();
    }
}
