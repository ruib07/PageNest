using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IOrdersService
{
    Task<IEnumerable<Order>> GetOrders();
    Task<IEnumerable<Order>> GetOrdersByUserId(Guid userId);
    Task<Result<Order>> GetOrderById(Guid orderId);
    Task<Result<Order>> CreateOrder(Order order);
    Task<Result<Order>> UpdateOrder(Guid orderId, Order updateOrder);
    Task DeleteOrder(Guid orderId); 
}
