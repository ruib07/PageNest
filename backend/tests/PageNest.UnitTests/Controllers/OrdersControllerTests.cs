using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Controllers;

public class OrdersControllerTests : TestBase
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrdersService _ordersService;
    private readonly OrdersController _ordersController;

    public OrdersControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _ordersService = new OrdersService(_orderRepositoryMock.Object);
        _ordersController = new OrdersController(_ordersService);
    }

    [Fact]
    public async Task GetOrders_ShouldReturnOkResult_WithAllOrders()
    {
        var orders = OrdersBuilder.CreateOrders();

        _orderRepositoryMock.Setup(repo => repo.GetOrders()).ReturnsAsync(orders);

        var result = await _ordersController.GetOrders();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Order>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(orders.Count, response.Count());
        });
    }

    [Fact]
    public async Task GetOrdersByUserId_ShouldReturnOkResult_WithAllOrders_WhenUserExists()
    {
        var orders = OrdersBuilder.CreateOrders();
        var ordersByUserList = orders.Where(o => o.UserId == orders[0].UserId).ToList();

        _orderRepositoryMock.Setup(repo => repo.GetOrdersByUserId(orders[0].UserId))
                                               .ReturnsAsync(ordersByUserList);

        var result = await _ordersController.GetOrdersByUserId(orders[0].UserId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Order>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(orders[0].UserId, response.First().UserId);
        });
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnOkResult_WithOrder()
    {
        var order = OrdersBuilder.CreateOrders().First();

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(order.Id)).ReturnsAsync(order);

        var result = await _ordersController.GetOrderById(order.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Order;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(order.Id, response.Id);
            Assert.Equal(order.UserId, response.UserId);
            Assert.Equal(order.Status, response.Status);
            Assert.Equal(order.Total, response.Total);
        });
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFoundResult_WhenOrderDoesNotExist()
    {
        var result = await _ordersController.GetOrderById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Order not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnCreatedResult_WhenOrderIsCreated() 
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

        var result = await _ordersController.CreateOrder(order);
        var createdResult = result as CreatedAtActionResult;
        var response = createdResult.Value as ResponsesDTO.Creation;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("Order created successfully.", response.Message);
            Assert.Equal(order.Id, response.Id);
        });
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderIsInvalid()
    {
        var invalidOrder = OrdersBuilder.InvalidOrderCreation(0, OrderStatus.Shipped);

        var result = await _ordersController.CreateOrder(invalidOrder);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Order must have at least one item.", error.Message);
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
            OrderItems = new List<OrderItem>()
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
            OrderItems = new List<OrderItem>()
            {
                new() { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = 2, PriceAtPurchase = 100m }
            }
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(existingOrder.Id)).ReturnsAsync(existingOrder);
        _orderRepositoryMock.Setup(repo => repo.UpdateOrder(It.IsAny<Order>())).Returns(Task.CompletedTask);

        var result = await _ordersController.UpdateOrder(existingOrder.Id, updatedOrder);
        var okResult = result as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Order updated successfully.", okResult.Value);
        });
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenValidationFails()
    {
        var existingOrder = OrdersBuilder.CreateOrders(1).First();
        var invalidUpdate = OrdersBuilder.InvalidOrderCreation(-10, OrderStatus.Cancelled);
        invalidUpdate.OrderItems = new List<OrderItem>()
        {
            new() { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = -1, PriceAtPurchase = -50 }
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(existingOrder.Id)).ReturnsAsync(existingOrder);

        var result = await _ordersController.UpdateOrder(existingOrder.Id, invalidUpdate);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Each item must have quantity greater than zero.", error.Message);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturnNoContentResult_WhenOrderIsDeleted()
    {
        var order = OrdersBuilder.CreateOrders().First();

        _orderRepositoryMock.Setup(repo => repo.DeleteOrder(order.Id)).Returns(Task.CompletedTask);

        var result = await _ordersController.DeleteOrder(order.Id);
        var noContentResult = result as NoContentResult;

        Assert.Equal(204, noContentResult.StatusCode);
    }
}
