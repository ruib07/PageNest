using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Controllers;

public class CartItemsControllerTests : TestBase
{
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly CartItemsService _cartItemsService;
    private readonly CartItemsController _cartItemsController;

    public CartItemsControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _cartItemsService = new CartItemsService(_cartItemRepositoryMock.Object);
        _cartItemsController = new CartItemsController(_cartItemsService);
    }

    [Fact]
    public async Task GetCartItems_ShouldReturnOkResult_WithAllCartItems()
    {
        var cartItems = CartItemsBuilder.CreateCartItems();

        _cartItemRepositoryMock.Setup(repo => repo.GetCartItems()).ReturnsAsync(cartItems);

        var result = await _cartItemsController.GetCartItems();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<CartItem>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(cartItems.Count, response.Count());
            Assert.Equal(cartItems.First().Id, response.First().Id);
            Assert.Equal(cartItems.Last().Id, response.Last().Id);
        });
    }

    [Fact]
    public async Task GetCartItemsByUserId_ShouldReturnOkResult_WithAllCartItems_WhenUserExists()
    {
        var cartItems = CartItemsBuilder.CreateCartItems();
        var cartItemsByUserList = cartItems.Where(ci => ci.UserId == cartItems[0].UserId).ToList();

        _cartItemRepositoryMock.Setup(repo => repo.GetCartItemsByUserId(cartItems[0].UserId))
                                                  .ReturnsAsync(cartItemsByUserList);

        var result = await _cartItemsController.GetCartItemsByUserId(cartItems[0].UserId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<CartItem>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(cartItems[0].UserId, response.First().UserId);
        });
    }

    [Fact]
    public async Task GetCartItemById_ShouldReturnOkResult_WithCartItem()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepositoryMock.Setup(repo => repo.GetCartItemById(cartItem.Id)).ReturnsAsync(cartItem);

        var result = await _cartItemsController.GetCartItemById(cartItem.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as CartItem;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(cartItem.Id, response.Id);
            Assert.Equal(cartItem.UserId, response.UserId);
            Assert.Equal(cartItem.BookId, response.BookId);
            Assert.Equal(cartItem.Quantity, response.Quantity);
        });
    }

    [Fact]
    public async Task GetCartItemById_ShouldReturnNotFoundResult_WhenCartItemDoesNotExist()
    {
        var result = await _cartItemsController.GetCartItemById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Cart item not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnCreatedResult_WhenCartItemIsCreated()
    {
        var newCartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepositoryMock.Setup(repo => repo.CreateCartItem(newCartItem)).ReturnsAsync(newCartItem);

        var result = await _cartItemsController.CreateCartItem(newCartItem);
        var createdResult = result as CreatedAtActionResult;
        var response = createdResult.Value as ResponsesDTO.Creation;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("Cart item created successfully.", response.Message);
            Assert.Equal(newCartItem.Id, response.Id);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenCartItemIsEmpty()
    {
        _cartItemRepositoryMock.Setup(repo => repo.CreateCartItem(null)).ReturnsAsync((CartItem)null);

        var result = await _cartItemsController.CreateCartItem(null);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Cart item cannot be null.", error.Message);
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenQuantityIsNegative()
    {
        var cartItem = CartItemsBuilder.InvalidCartItemCreation(-1);

        _cartItemRepositoryMock.Setup(repo => repo.CreateCartItem(cartItem)).ReturnsAsync(cartItem);

        var result = await _cartItemsController.CreateCartItem(cartItem);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Quantity must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenQuantityIsZero()
    {
        var cartItem = CartItemsBuilder.InvalidCartItemCreation(0);

        _cartItemRepositoryMock.Setup(repo => repo.CreateCartItem(cartItem)).ReturnsAsync(cartItem);

        var result = await _cartItemsController.CreateCartItem(cartItem);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Quantity must be greater than zero.", error.Message);
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

        _cartItemRepositoryMock.Setup(repo => repo.GetCartItemsByUserId(newCartItem.UserId))
                                                  .ReturnsAsync(new List<CartItem> { existingCartItem });

        var result = await _cartItemsController.CreateCartItem(newCartItem);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(409, error.StatusCode);
        Assert.Equal("Book already exists in the cart.", error.Message);
    }

    [Fact]
    public async Task UpdateCartItem_ShouldReturnOkResult_WhenCartItemIsUpdated()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();
        var updatedCartItem = CartItemsBuilder.UpdateCartItem(cartItem.Id, cartItem.UserId, cartItem.BookId);

        _cartItemRepositoryMock.Setup(repo => repo.GetCartItemById(cartItem.Id)).ReturnsAsync(cartItem);
        _cartItemRepositoryMock.Setup(repo => repo.UpdateCartItem(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

        var result = await _cartItemsController.UpdateCartItem(cartItem.Id, updatedCartItem);
        var okResult = result as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Cart item updated successfully.", okResult.Value);
        });
    }

    [Fact]
    public async Task UpdateCartItem_ShouldReturnBadRequest_WhenCartItemIsNull()
    {
        var existingCartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepositoryMock.Setup(repo => repo.GetCartItemById(existingCartItem.Id)).ReturnsAsync(existingCartItem);

        var result = await _cartItemsController.UpdateCartItem(existingCartItem.Id, null);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Cart item cannot be null.", error.Message);
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

        _cartItemRepositoryMock.Setup(repo => repo.GetCartItemById(existingCartItem.Id))
                                                  .ReturnsAsync(existingCartItem);

        var result = await _cartItemsController.UpdateCartItem(existingCartItem.Id, invalidCartItem);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Quantity must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task DeleteCartItem_ShouldReturnNoContentResult_WhenCartItemIsDeleted()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        _cartItemRepositoryMock.Setup(repo => repo.DeleteCartItem(cartItem.Id)).Returns(Task.CompletedTask);

        var result = await _cartItemsController.DeleteCartItem(cartItem.Id);
        var noContentResult = result as NoContentResult;

        Assert.Equal(204, noContentResult.StatusCode);
    }
}
