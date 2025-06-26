using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class OrderItemsService : IOrderItemsService
{
    private readonly IOrderItemRepository _orderItemRepository;

    public OrderItemsService(IOrderItemRepository orderItemRepository)
    {
        _orderItemRepository = orderItemRepository;
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItems()
    {
        return await _orderItemRepository.GetOrderItems();
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByBookId(Guid bookId)
    {
        return await _orderItemRepository.GetOrderItemsByBookId(bookId);
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(Guid orderId)
    {
        return await _orderItemRepository.GetOrderItemsByOrderId(orderId);
    }

    public async Task<Result<OrderItem>> GetOrderItemById(Guid orderItemId)
    {
        var orderItem = await _orderItemRepository.GetOrderItemById(orderItemId);

        if (orderItem == null)
            return Result<OrderItem>.Fail("Order item not found.", 404);

        return Result<OrderItem>.Success(orderItem);
    }
}
