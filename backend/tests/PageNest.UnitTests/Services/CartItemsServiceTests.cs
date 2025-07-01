using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class CartItemsServiceTests : TestBase
{
    private readonly Mock<ICartItemRepository> _cartItemRepository;
    private readonly CartItemsService _cartItemsService;

    public CartItemsServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _cartItemRepository = new Mock<ICartItemRepository>();
        _cartItemsService = new CartItemsService(_cartItemRepository.Object);
    }

    [Fact]
    public async Task GetCartItems_ShouldReturnAllCartItems()
    {
        var cartItems = CartItemsBuilder.CreateCartItems();
        await _context.CartItems.AddRangeAsync(cartItems);
        await _context.SaveChangesAsync();

        _cartItemRepository.Setup(repo => repo.GetCartItems()).ReturnsAsync(cartItems);

        var result = await _cartItemsService.GetCartItems();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(cartItems.Count, result.Count());
            Assert.Equal(cartItems.First().Id, result.First().Id);
            Assert.Equal(cartItems.Last().Id, result.Last().Id);
        });
    }

    [Fact]
    public async Task GetCartItemsByUserId_ShouldReturnCartItems_WhenUserExists()
    {
        var cartItems = CartItemsBuilder.CreateCartItems();
        var cartItemsByUserList = cartItems.Where(ci => ci.UserId == cartItems[0].UserId).ToList();

        _cartItemRepository.Setup(repo => repo.GetCartItemsByUserId(cartItems[0].UserId)).ReturnsAsync(cartItemsByUserList);

        var result = await _cartItemsService.GetCartItemsByUserId(cartItems[0].UserId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(cartItems.First().UserId, result.First().UserId);
    }

    [Fact]
    public async Task GetCartItemById_ShouldReturnCartItem_WhenCartItemExists()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepository.Setup(repo => repo.GetCartItemById(cartItem.Id)).ReturnsAsync(cartItem);

        var result = await _cartItemsService.GetCartItemById(cartItem.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(cartItem.Id, result.Data.Id);
            Assert.Equal(cartItem.UserId, result.Data.UserId);
            Assert.Equal(cartItem.BookId, result.Data.BookId);
            Assert.Equal(cartItem.Quantity, result.Data.Quantity);
        });
    }

    [Fact]
    public async Task GetCartItemById_ShouldReturnNotFound_WhenCartItemDoesNotExist()
    {
        _cartItemRepository.Setup(repo => repo.GetCartItemById(It.IsAny<Guid>())).ReturnsAsync((CartItem)null);

        var result = await _cartItemsService.GetCartItemById(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Cart item not found.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldCreateCartItemSuccessfully()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepository.Setup(repo => repo.CreateCartItem(cartItem)).ReturnsAsync(cartItem);

        var result = await _cartItemsService.CreateCartItem(cartItem);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(cartItem.Id, result.Data.Id);
            Assert.Equal("Cart item created successfully.", result.Message);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenCartItemIsEmpty()
    {
        _cartItemRepository.Setup(repo => repo.CreateCartItem(null)).ReturnsAsync((CartItem)null);

        var result = await _cartItemsService.CreateCartItem(null);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Cart item cannot be null.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenQuantityIsNegative()
    {
        var cartItem = CartItemsBuilder.InvalidCartItemCreation(-1);

        _cartItemRepository.Setup(repo => repo.CreateCartItem(cartItem)).ReturnsAsync(cartItem);

        var result = await _cartItemsService.CreateCartItem(cartItem);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Quantity must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenQuantityIsZero()
    {
        var cartItem = CartItemsBuilder.InvalidCartItemCreation(0);

        _cartItemRepository.Setup(repo => repo.CreateCartItem(cartItem)).ReturnsAsync(cartItem);

        var result = await _cartItemsService.CreateCartItem(cartItem);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Quantity must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenCartItemHasDuplicateItems()
    {
        var existingCartItem = CartItemsBuilder.CreateCartItems().First();
        var newCartItem = new CartItem()
        {
            Id = Guid.NewGuid(),
            UserId = existingCartItem.UserId,
            BookId = existingCartItem.BookId,
            Quantity = 1
        };

        _cartItemRepository.Setup(repo => repo.GetCartItemsByUserId(newCartItem.UserId))
                                              .ReturnsAsync(new List<CartItem> { existingCartItem });

        var result = await _cartItemsService.CreateCartItem(newCartItem);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.Error.StatusCode);
            Assert.Equal("Book already exists in the cart.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateCartItem_ShouldUpdateCartItemSuccessfully()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();
        var updatedCartItem = CartItemsBuilder.UpdateCartItem(cartItem.Id, cartItem.UserId, cartItem.BookId);

        _cartItemRepository.Setup(repo => repo.CreateCartItem(cartItem)).ReturnsAsync(cartItem);
        _cartItemRepository.Setup(repo => repo.UpdateCartItem(updatedCartItem)).Returns(Task.CompletedTask);
        _cartItemRepository.Setup(repo => repo.GetCartItemById(cartItem.Id)).ReturnsAsync(cartItem);

        var updatedResult = await _cartItemsService.UpdateCartItem(cartItem.Id, updatedCartItem);
        var result = await _cartItemsService.GetCartItemById(cartItem.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedCartItem.Id, result.Data.Id);
            Assert.Equal("Cart item updated successfully.", updatedResult.Message);
        });
    }

    [Fact]
    public async Task UpdateCartItem_ShouldReturnBadRequest_WhenCartItemIsNull()
    {
        var existingCartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepository.Setup(repo => repo.GetCartItemById(existingCartItem.Id)).ReturnsAsync(existingCartItem);

        var result = await _cartItemsService.UpdateCartItem(existingCartItem.Id, null);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Cart item cannot be null.", result.Error.Message);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task UpdateCartItem_ShouldReturnBadRequest_WhenQuantityIsInvalid(int invalidQuantity)
    {
        var existingCartItem = CartItemsBuilder.CreateCartItems().First();

        var invalidCartItem = new CartItem()
        {
            Id = existingCartItem.Id,
            UserId = existingCartItem.UserId,
            BookId = existingCartItem.BookId,
            Quantity = invalidQuantity
        };

        _cartItemRepository.Setup(repo => repo.GetCartItemById(existingCartItem.Id)).ReturnsAsync(existingCartItem);

        var result = await _cartItemsService.UpdateCartItem(existingCartItem.Id, invalidCartItem);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Quantity must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task DeleteCartItem_ShouldDeleteCartItemSuccessfully()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepository.Setup(repo => repo.CreateCartItem(cartItem)).ReturnsAsync(cartItem);
        _cartItemRepository.Setup(repo => repo.DeleteCartItem(cartItem.Id)).Returns(Task.CompletedTask);
        _cartItemRepository.Setup(repo => repo.GetCartItemById(cartItem.Id)).ReturnsAsync((CartItem)null);

        await _cartItemsService.CreateCartItem(cartItem);
        await _cartItemsService.DeleteCartItem(cartItem.Id);

        var result = await _cartItemsService.GetCartItemById(cartItem.Id);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Cart item not found.", result.Error.Message);
        });
    }
}
