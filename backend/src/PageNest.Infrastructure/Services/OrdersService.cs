using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;

namespace PageNest.Infrastructure.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrderRepository _orderRepository;

    public OrdersService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<Order>> GetOrders()
    {
        return await _orderRepository.GetOrders();
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserId(Guid userId)
    {
        return await _orderRepository.GetOrdersByUserId(userId);
    }

    public async Task<Result<Order>> GetOrderById(Guid orderId)
    {
        var order = await _orderRepository.GetOrderById(orderId);

        if (order == null) return Result<Order>.Fail("Order not found.", 404);

        return Result<Order>.Success(order);
    }

    public async Task<Result<Order>> CreateOrder(Order order)
    {
        var validation = ValidateOrderFields(order);

        if (!validation.IsSuccess)
            return Result<Order>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var createdOrder = await _orderRepository.CreateOrder(order);

        return Result<Order>.Success(createdOrder, "Order created successfully.");
    }

    public async Task<Result<Order>> UpdateOrder(Guid orderId, Order updateOrder)
    {
        var currentOrder = await _orderRepository.GetOrderById(orderId);

        var validation = ValidateOrderFields(updateOrder);

        if (!validation.IsSuccess)
            return Result<Order>.Fail(validation.Error.Message, validation.Error.StatusCode);

        currentOrder.Status = updateOrder.Status;
        currentOrder.Total = updateOrder.Total;

        currentOrder.OrderItems.Clear();

        foreach (var item in updateOrder.OrderItems)
        {
            currentOrder.OrderItems.Add(new OrderItem()
            {
                Id = Guid.NewGuid(),
                BookId = item.BookId,
                Quantity = item.Quantity,
                PriceAtPurchase = item.PriceAtPurchase
            });
        }

        await _orderRepository.UpdateOrder(currentOrder);

        return Result<Order>.Success(currentOrder, "Order updated successfully.");
    }

    public async Task DeleteOrder(Guid orderId)
    {
        await _orderRepository.DeleteOrder(orderId);
    }

    #region Private Methods

    private static Result<bool> ValidateOrderFields(Order order)
    {
        if (!Enum.IsDefined(typeof(Status), order.Status))
            return Result<bool>.Fail("Invalid status.", 400);

        if (order.OrderItems == null || !order.OrderItems.Any())
            return Result<bool>.Fail("Order must have at least one item.", 400);

        foreach (var item in order.OrderItems)
        {
            if (item.Quantity <= 0)
                return Result<bool>.Fail("Each item must have quantity greater than zero.", 400);

            if (item.PriceAtPurchase < 0)
                return Result<bool>.Fail("Item price cannot be negative.", 400);
        }

        var expectedTotal = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase);

        if (order.Total != expectedTotal)
            return Result<bool>.Fail("Order total does not match the sum of the items.", 400);

        if (order.Total < 0)
            return Result<bool>.Fail("Total cannot be negative.", 400);

        return Result<bool>.Success(true);
    }

    #endregion
}
