using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class OrderItemRepositoryTests : TestBase
{
    private readonly OrderItemRepository _orderItemRepository;

    public OrderItemRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _orderItemRepository = new OrderItemRepository(_context);
    }

    [Fact]
    public async Task GetOrderItems_ShouldReturnAllOrderItems()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        await _context.OrderItems.AddRangeAsync(orderItems);
        await _context.SaveChangesAsync();

        var result = await _orderItemRepository.GetOrderItems();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(orderItems.Count, result.Count());
            Assert.Equal(orderItems.First().OrderId, result.First().OrderId);
            Assert.Equal(orderItems.First().BookId, result.First().BookId);
            Assert.Equal(orderItems.First().Quantity, result.First().Quantity);
            Assert.Equal(orderItems.First().PriceAtPurchase, result.First().PriceAtPurchase);
            Assert.Equal(orderItems.Last().OrderId, result.Last().OrderId);
            Assert.Equal(orderItems.Last().BookId, result.Last().BookId);
            Assert.Equal(orderItems.Last().Quantity, result.Last().Quantity);
            Assert.Equal(orderItems.Last().PriceAtPurchase, result.Last().PriceAtPurchase);
        });
    }

    [Fact]
    public async Task GetOrderItemsByOrderId_ShouldReturnOrderItems_WhenOrderExists()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        await _context.OrderItems.AddRangeAsync(orderItems);
        await _context.SaveChangesAsync();

        var result = await _orderItemRepository.GetOrderItemsByOrderId(orderItems.First().OrderId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(orderItems.First().OrderId, result.First().OrderId);
    }

    [Fact]
    public async Task GetOrderItemsByBookId_ShouldReturnOrderItems_WhenBookExists()
    {
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        await _context.OrderItems.AddRangeAsync(orderItems);
        await _context.SaveChangesAsync();

        var result = await _orderItemRepository.GetOrderItemsByBookId(orderItems.First().BookId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(orderItems.First().BookId, result.First().BookId);
    }

    [Fact]
    public async Task GetOrderItemById_ShouldReturnOrderItem_WhenOrderItemExists()
    {
        var orderItem = OrderItemsBuilder.CreateOrderItems().First();
        await _context.OrderItems.AddAsync(orderItem);
        await _context.SaveChangesAsync();

        var result = await _orderItemRepository.GetOrderItemById(orderItem.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(orderItem.Id, result.Id);
            Assert.Equal(orderItem.OrderId, result.OrderId);
            Assert.Equal(orderItem.BookId, result.BookId);
            Assert.Equal(orderItem.Quantity, result.Quantity);
            Assert.Equal(orderItem.PriceAtPurchase, result.PriceAtPurchase);
        });
    }
}
