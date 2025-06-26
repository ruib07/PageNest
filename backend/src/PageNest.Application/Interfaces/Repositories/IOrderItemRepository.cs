using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IOrderItemRepository
{
    Task<IEnumerable<OrderItem>> GetOrderItems();
    Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(Guid orderId);
    Task<IEnumerable<OrderItem>> GetOrderItemsByBookId(Guid bookId);
    Task<OrderItem> GetOrderItemById(Guid orderItemId);
}
