using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class OrdersServiceTests : TestBase
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrdersService _ordersService;

    public OrdersServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _ordersService = new OrdersService(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task GetOrders_ShouldReturnAllOrders()
    {
        var orders = OrdersBuilder.CreateOrders();
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        _orderRepositoryMock.Setup(repo => repo.GetOrders()).ReturnsAsync(orders);

        var result = await _ordersService.GetOrders();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(orders.Count, result.Count());
            Assert.Equal(orders.First().Id, result.First().Id);
            Assert.Equal(orders.Last().Id, result.Last().Id);
        });
    }

    [Fact]
    public async Task GetOrdersByUserId_ShouldReturnOrders_WhenUserExists()
    {
        var orders = OrdersBuilder.CreateOrders();
        var ordersByUserList = orders.Where(o => o.UserId == orders[0].UserId).ToList();

        _orderRepositoryMock.Setup(repo => repo.GetOrdersByUserId(orders[0].UserId)).ReturnsAsync(ordersByUserList);

        var result = await _ordersService.GetOrdersByUserId(orders[0].UserId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(orders.First().Id, result.First().Id);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
    {
        var order = OrdersBuilder.CreateOrders(1).First();

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(order.Id)).ReturnsAsync(order);

        var result = await _ordersService.GetOrderById(order.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(order.Id, result.Data.Id);
            Assert.Equal(order.Total, result.Data.Total);
        });
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        _orderRepositoryMock.Setup(repo => repo.GetOrderById(It.IsAny<Guid>())).ReturnsAsync((Order)null);

        var result = await _ordersService.GetOrderById(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Order not found.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateOrder_ShouldCreateOrderSuccessfully()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            Total = 100m,
            OrderItems = new List<OrderItem>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    BookId = Guid.NewGuid(),
                    Quantity = 2,
                    PriceAtPurchase = 50m
                }
            }
        };

        _orderRepositoryMock.Setup(repo => repo.CreateOrder(order)).ReturnsAsync(order);

        var result = await _ordersService.CreateOrder(order);

        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(order.Id, result.Data.Id);
            Assert.Equal("Order created successfully.", result.Message);
        });
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderIsInvalid()
    {
        var invalidOrder = OrdersBuilder.InvalidOrderCreation(0, OrderStatus.Shipped);

        var result = await _ordersService.CreateOrder(invalidOrder);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Order must have at least one item.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateSuccessfully_WhenOrderIsValid()
    {
        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            Total = 100m,
            OrderItems = new List<OrderItem>
            {
                new() { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = 1, PriceAtPurchase = 100m }
            }
        };

        var updatedOrder = new Order
        {
            Id = existingOrder.Id,
            UserId = existingOrder.UserId,
            Status = OrderStatus.Delivered,
            Total = 200m,
            OrderItems = new List<OrderItem>
            {
                new() { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = 2, PriceAtPurchase = 100m }
            }
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(existingOrder.Id)).ReturnsAsync(existingOrder);
        _orderRepositoryMock.Setup(repo => repo.UpdateOrder(It.IsAny<Order>())).Returns(Task.CompletedTask);

        var result = await _ordersService.UpdateOrder(existingOrder.Id, updatedOrder);

        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.Equal("Order updated successfully.", result.Message);
            Assert.Equal(updatedOrder.Total, result.Data.Total);
        });
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenValidationFails()
    {
        var existingOrder = OrdersBuilder.CreateOrders(1).First();
        var invalidUpdate = OrdersBuilder.InvalidOrderCreation(-10, OrderStatus.Cancelled);
        invalidUpdate.OrderItems = new List<OrderItem>
        {
            new() { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = -1, PriceAtPurchase = -50 }
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(existingOrder.Id)).ReturnsAsync(existingOrder);

        var result = await _ordersService.UpdateOrder(existingOrder.Id, invalidUpdate);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Each item must have quantity greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task DeleteOrder_ShouldDeleteOrderSuccessfully()
    {
        var order = OrdersBuilder.CreateOrders().First();

        _orderRepositoryMock.Setup(repo => repo.CreateOrder(order)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(repo => repo.DeleteOrder(order.Id)).Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(repo => repo.GetOrderById(order.Id)).ReturnsAsync((Order)null);

        await _ordersService.CreateOrder(order);
        await _ordersService.DeleteOrder(order.Id);

        var result = await _ordersService.GetOrderById(order.Id);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Order not found.", result.Error.Message);
        });
    }
}
