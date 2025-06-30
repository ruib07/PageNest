using PageNest.Domain.Entities;

namespace PageNest.TestUtils.Builders;

public class OrderItemsBuilder
{
    private static int _counter = 2;

    public static List<OrderItem> CreateOrderItems(int quantity = 2)
    {
        var orderItems = new List<OrderItem>();

        for (int i = 0; i < quantity; i++)
        {
            orderItems.Add(new OrderItem()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                BookId = Guid.NewGuid(),
                Quantity = _counter,
                PriceAtPurchase = 19.99m + _counter
            });

            _counter++;
        }

        return orderItems;
    }
}
