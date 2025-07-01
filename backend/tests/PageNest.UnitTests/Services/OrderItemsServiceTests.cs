using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class OrderItemsServiceTests : TestBase
{
    private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
    private readonly OrderItemsService _orderItemsService;

    public OrderItemsServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
        _orderItemsService = new OrderItemsService(_orderItemRepositoryMock.Object);
    }

    [Fact]
    public async Task GetOrderItems_ShouldReturnAllOrderItems()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        await _context.OrderItems.AddRangeAsync(orderItems);
        await _context.SaveChangesAsync();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItems()).ReturnsAsync(orderItems);

        var result = await _orderItemsService.GetOrderItems();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(orderItems.Count, result.Count());
            Assert.Equal(orderItems.First().Id, result.First().Id);
            Assert.Equal(orderItems.First().OrderId, result.First().OrderId);
            Assert.Equal(orderItems.First().BookId, result.First().BookId);
            Assert.Equal(orderItems.First().Quantity, result.First().Quantity);
            Assert.Equal(orderItems.First().PriceAtPurchase, result.First().PriceAtPurchase);
            Assert.Equal(orderItems.Last().Id, result.Last().Id);
            Assert.Equal(orderItems.Last().OrderId, result.Last().OrderId);
            Assert.Equal(orderItems.Last().BookId, result.Last().BookId);
            Assert.Equal(orderItems.Last().Quantity, result.Last().Quantity);
            Assert.Equal(orderItems.Last().PriceAtPurchase, result.Last().PriceAtPurchase);
        });
    }

    [Fact]
    public async Task GetOrderItemsByBookId_ShouldReturnOrderItems_WhenBookExists()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        var orderItemsByBookList = orderItems.Where(p => p.BookId == orderItems[0].BookId).ToList();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItemsByBookId(orderItems.First().BookId))
                                                   .ReturnsAsync(orderItemsByBookList);

        var result = await _orderItemsService.GetOrderItemsByBookId(orderItems.First().BookId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(orderItems.First().BookId, result.First().BookId);
    }

    [Fact]
    public async Task GetOrderItemsByOrderId_ShouldReturnOrderItems_WhenOrderExists()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        var orderItemsByOrderList = orderItems.Where(p => p.OrderId == orderItems[0].OrderId).ToList();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItemsByOrderId(orderItems.First().OrderId))
                                                   .ReturnsAsync(orderItemsByOrderList);

        var result = await _orderItemsService.GetOrderItemsByOrderId(orderItems.First().OrderId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(orderItems.First().OrderId, result.First().OrderId);
    }

    [Fact]
    public async Task GetOrderItemById_ShouldReturnOrderItem_WhenOrderItemExists()
    {
        var orderItem = OrderItemsBuilder.CreateOrderItems().First();

        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItemById(orderItem.Id)).ReturnsAsync(orderItem);

        var result = await _orderItemsService.GetOrderItemById(orderItem.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(orderItem.Id, result.Data.Id);
            Assert.Equal(orderItem.OrderId, result.Data.OrderId);
            Assert.Equal(orderItem.BookId, result.Data.BookId);
            Assert.Equal(orderItem.Quantity, result.Data.Quantity);
            Assert.Equal(orderItem.PriceAtPurchase, result.Data.PriceAtPurchase);
        });
    }

    [Fact]
    public async Task GetOrderItemById_ShouldReturnNotFound_WhenOrderItemDoesNotExist()
    {
        _orderItemRepositoryMock.Setup(repo => repo.GetOrderItemById(It.IsAny<Guid>())).ReturnsAsync((OrderItem)null);

        var result = await _orderItemsService.GetOrderItemById(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Order item not found.", result.Error.Message);
        });
    }
}
