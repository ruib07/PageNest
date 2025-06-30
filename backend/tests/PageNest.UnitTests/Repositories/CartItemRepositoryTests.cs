using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class CartItemRepositoryTests : TestBase
{
    private readonly CartItemRepository _cartItemRepository;

    public CartItemRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _cartItemRepository = new CartItemRepository(_context);
    }

    [Fact]
    public async Task GetCartItems_ShouldReturnAllCartItems()
    {
        var cartItems = CartItemsBuilder.CreateCartItems();
        await _context.CartItems.AddRangeAsync(cartItems);
        await _context.SaveChangesAsync();

        var result = await _cartItemRepository.GetCartItems();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(cartItems.Count, result.Count());
            Assert.Equal(cartItems.First().Id, result.First().Id);
            Assert.Equal(cartItems.First().UserId, result.First().UserId);
            Assert.Equal(cartItems.First().BookId, result.First().BookId);
            Assert.Equal(cartItems.First().Quantity, result.First().Quantity);
            Assert.Equal(cartItems.Last().Id, result.Last().Id);
            Assert.Equal(cartItems.Last().UserId, result.Last().UserId);
            Assert.Equal(cartItems.Last().BookId, result.Last().BookId);
            Assert.Equal(cartItems.Last().Quantity, result.Last().Quantity);
        });
    }

    [Fact]
    public async Task GetCartItemsByUserId_ShouldReturnAllCartItems_WhenUserExists()
    {
        var cartItems = CartItemsBuilder.CreateCartItems();
        await _context.CartItems.AddRangeAsync(cartItems);
        await _context.SaveChangesAsync();

        var result = await _cartItemRepository.GetCartItemsByUserId(cartItems.First().UserId);

        Assert.NotNull(result);
        Assert.Equal(cartItems.First().Id, result.First().Id);
        Assert.Equal(cartItems.First().UserId, result.First().UserId);
    }

    [Fact]
    public async Task GetCartItemById_ShouldReturnCartItem_WhenCartItemExists()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();
        await _cartItemRepository.CreateCartItem(cartItem);

        var result = await _cartItemRepository.GetCartItemById(cartItem.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(cartItem.Id, result.Id);
            Assert.Equal(cartItem.UserId, result.UserId);
            Assert.Equal(cartItem.BookId, result.BookId);
            Assert.Equal(cartItem.Quantity, result.Quantity);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldCreateCartItem()
    {
        var newCartItem = CartItemsBuilder.CreateCartItems().First();
        await _cartItemRepository.CreateCartItem(newCartItem);

        var result = await _cartItemRepository.GetCartItemById(newCartItem.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(newCartItem.Id, result.Id);
            Assert.Equal(newCartItem.UserId, result.UserId);
            Assert.Equal(newCartItem.BookId, result.BookId);
            Assert.Equal(newCartItem.Quantity, result.Quantity);
        });
    }

    [Fact]
    public async Task UpdateCartItem_ShouldUpdateCartItem()
    {
        var createCartItem = CartItemsBuilder.CreateCartItems().First();
        await _cartItemRepository.CreateCartItem(createCartItem);

        _context.Entry(createCartItem).State = EntityState.Detached;

        var updatedCartItem = CartItemsBuilder.UpdateCartItem(createCartItem.Id, createCartItem.UserId, createCartItem.BookId);
        await _cartItemRepository.UpdateCartItem(updatedCartItem);

        var result = await _cartItemRepository.GetCartItemById(createCartItem.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedCartItem.Id, result.Id);
            Assert.Equal(updatedCartItem.UserId, result.UserId);
            Assert.Equal(updatedCartItem.BookId, result.BookId);
            Assert.Equal(updatedCartItem.Quantity, result.Quantity);
        });
    }

    [Fact]
    public async Task DeleteCartItem_ShouldDeleteCartItem()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        await _cartItemRepository.CreateCartItem(cartItem);
        await _cartItemRepository.DeleteCartItem(cartItem.Id);

        var result = await _cartItemRepository.GetCartItemById(cartItem.Id);

        Assert.Null(result);
    }
}
