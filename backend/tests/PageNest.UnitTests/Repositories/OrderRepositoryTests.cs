using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class OrderRepositoryTests : TestBase
{
    private readonly OrderRepository _orderRepository;

    public OrderRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _orderRepository = new OrderRepository(_context);
    }

    [Fact]
    public async Task GetOrders_ShouldReturnAllOrders()
    {
        var orders = OrdersBuilder.CreateOrders();
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        var result = await _orderRepository.GetOrders();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(orders.Count, result.Count());
            Assert.Equal(orders.First().Id, result.First().Id);
            Assert.Equal(orders.First().UserId, result.First().UserId);
            Assert.Equal(orders.First().Status, result.First().Status);
            Assert.Equal(orders.First().Total, result.First().Total);
            Assert.Equal(orders.Last().Id, result.Last().Id);
            Assert.Equal(orders.Last().UserId, result.Last().UserId);
            Assert.Equal(orders.Last().Status, result.Last().Status);
            Assert.Equal(orders.Last().Total, result.Last().Total);
        });
    }

    [Fact]
    public async Task GetOrdersByUserId_ShouldReturnAllOrders_WhenUserExists()
    {
        var orders = OrdersBuilder.CreateOrders();
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        var result = await _orderRepository.GetOrdersByUserId(orders.First().UserId);

        Assert.NotNull(result);
        Assert.Equal(orders.First().Id, result.First().Id);
        Assert.Equal(orders.First().UserId, result.First().UserId);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
    {
        var order = OrdersBuilder.CreateOrders().First();
        await _orderRepository.CreateOrder(order);

        var result = await _orderRepository.GetOrderById(order.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(order.Id, result.Id);
            Assert.Equal(order.UserId, result.UserId);
            Assert.Equal(order.Status, result.Status);
            Assert.Equal(order.Total, result.Total);
        });
    }

    [Fact]
    public async Task CreateOrder_ShouldCreateOrder()
    {
        var newOrder = OrdersBuilder.CreateOrders().First();
        await _orderRepository.CreateOrder(newOrder);

        var result = await _orderRepository.GetOrderById(newOrder.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(newOrder.Id, result.Id);
            Assert.Equal(newOrder.UserId, result.UserId);
            Assert.Equal(newOrder.Status, result.Status);
            Assert.Equal(newOrder.Total, result.Total);
        });
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateOrder()
    {
        var createOrder = OrdersBuilder.CreateOrders().First();
        await _orderRepository.CreateOrder(createOrder);

        _context.Entry(createOrder).State = EntityState.Detached;

        var updatedOrder = OrdersBuilder.UpdateOrder(createOrder.Id, createOrder.UserId);
        await _orderRepository.UpdateOrder(updatedOrder);

        var result = await _orderRepository.GetOrderById(createOrder.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedOrder.Id, result.Id);
            Assert.Equal(updatedOrder.UserId, result.UserId);
            Assert.Equal(updatedOrder.Status, result.Status);
            Assert.Equal(updatedOrder.Total, result.Total);
        });
    }

    [Fact]
    public async Task DeleteOrder_ShouldDeleteOrder()
    {
        var order = OrdersBuilder.CreateOrders().First();

        await _orderRepository.CreateOrder(order);
        await _orderRepository.DeleteOrder(order.Id);

        var result = await _orderRepository.GetOrderById(order.Id);

        Assert.Null(result);
    }
}
