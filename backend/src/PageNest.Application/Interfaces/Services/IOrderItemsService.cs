using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IOrderItemsService
{
    Task<IEnumerable<OrderItem>> GetOrderItems();
    Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(Guid orderId);
    Task<IEnumerable<OrderItem>> GetOrderItemsByBookId(Guid bookId);
    Task<Result<OrderItem>> GetOrderItemById(Guid orderItemId);
}
