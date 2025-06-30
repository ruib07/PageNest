using PageNest.Domain.Entities;
using PageNest.Domain.Enums;

namespace PageNest.TestUtils.Builders;

public class OrdersBuilder
{
    private static int _counter = 2;

    public static List<Order> CreateOrders(int quantity = 2)
    {
        var orders = new List<Order>();

        for (int i = 0; i < quantity; i++)
        {
            orders.Add(new Order()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                Total = 100.00m + _counter
            });

            _counter++;
        }

        return orders;
    }

    public static Order InvalidOrderCreation(decimal total, OrderStatus status)
    {
        return new Order()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = status,
            Total = total
        };
    }

    public static Order UpdateOrder(Guid id, Guid userId)
    {
        return new Order()
        {
            Id = id,
            UserId = userId,
            Status = OrderStatus.Delivered,
            Total = 150.00m
        };
    }
}
