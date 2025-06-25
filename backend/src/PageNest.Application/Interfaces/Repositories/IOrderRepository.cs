using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetOrders();
    Task<IEnumerable<Order>> GetOrdersByUserId(Guid userId);
    Task<Order> GetOrderById(Guid orderId);
    Task<Order> CreateOrder(Order order);
    Task UpdateOrder(Order order);
    Task DeleteOrder(Guid orderId);
}
