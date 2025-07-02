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

public class OrderItemsControllerTests : TestBase
{
    private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
    private readonly OrderItemsService _orderItemsService;
    private readonly OrderItemsController _orderItemsController;

    public OrderItemsControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
        _orderItemsService = new OrderItemsService(_orderItemRepositoryMock.Object);
        _orderItemsController = new OrderItemsController(_orderItemsService);
    }

    [Fact]
    public async Task GetOrderItems_ShouldReturnOkResult_WithAllOrderItems()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItems()).ReturnsAsync(orderItems);

        var result = await _orderItemsController.GetOrderItems();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<OrderItem>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(orderItems.Count, response.Count());
            Assert.Equal(orderItems.First().Id, response.First().Id);
            Assert.Equal(orderItems.Last().Id, response.Last().Id);
        });
    }

    [Fact]
    public async Task GetOrderItemsByOrderId_ShouldReturnOkResult_WithAllOrderItems_WhenOrderExists()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        var orderItemsByOrderList = orderItems.Where(p => p.OrderId == orderItems[0].OrderId).ToList();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItemsByOrderId(orderItems[0].OrderId))
                                                   .ReturnsAsync(orderItemsByOrderList);

        var result = await _orderItemsController.GetOrderItemsByOrderId(orderItems[0].OrderId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<OrderItem>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(orderItems[0].OrderId, response.First().OrderId);
        });
    }

    [Fact]
    public async Task GetOrderItemsByBookId_ShouldReturnOkResult_WithAllOrderItems_WhenBookExists()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        var orderItemsByBookList = orderItems.Where(p => p.BookId == orderItems[0].BookId).ToList();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItemsByBookId(orderItems[0].BookId))
                                                   .ReturnsAsync(orderItemsByBookList);

        var result = await _orderItemsController.GetOrderItemsByBookId(orderItems[0].BookId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<OrderItem>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(orderItems[0].BookId, response.First().BookId);
        });
    }

    [Fact]
    public async Task GetOrderItemById_ShouldReturnOkResult_WithOrderItem()
    {
        var orderItem = OrderItemsBuilder.CreateOrderItems().First();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItemById(orderItem.Id)).ReturnsAsync(orderItem);

        var result = await _orderItemsController.GetOrderItemById(orderItem.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as OrderItem;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(orderItem.Id, response.Id);
            Assert.Equal(orderItem.OrderId, response.OrderId);
            Assert.Equal(orderItem.BookId, response.BookId);
            Assert.Equal(orderItem.Quantity, response.Quantity);
            Assert.Equal(orderItem.PriceAtPurchase, response.PriceAtPurchase);
        });
    }

    [Fact]
    public async Task GetOrderItemById_ShouldReturnNotFoundResult_WhenOrderItemDoesNotExist()
    {
        var result = await _orderItemsController.GetOrderItemById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Order item not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }
}
