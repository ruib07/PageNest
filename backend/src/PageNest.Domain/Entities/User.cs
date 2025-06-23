using PageNest.Domain.Enums;

namespace PageNest.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Roles Role { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<PasswordReset> PasswordResets { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
}
