using PageNest.Domain.Entities;

namespace PageNest.TestUtils.Builders;

public class CartItemsBuilder
{
    private static int _counter = 2;

    public static List<CartItem> CreateCartItems(int quantity = 2)
    {
        var cartItems = new List<CartItem>();

        for (int i = 0; i < quantity; i++)
        {
            cartItems.Add(new CartItem()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                BookId = Guid.NewGuid(),
                Quantity = _counter,
            });

            _counter++;
        }

        return cartItems;
    }

    public static CartItem InvalidCartItemCreation(int quantity)
    {
        return new CartItem()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Quantity = quantity
        };
    }

    public static CartItem UpdateCartItem(Guid id, Guid userId, Guid bookId)
    {
        return new CartItem()
        {
            Id = id,
            UserId = userId,
            BookId = bookId,
            Quantity = 14
        };
    }
}
