using PageNest.Domain.Enums;

namespace PageNest.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Status Status { get; set; } 
    public decimal Total { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}
