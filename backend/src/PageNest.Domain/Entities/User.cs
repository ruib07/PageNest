using PageNest.Domain.Enums;

namespace PageNest.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Roles Role { get; set; }
    public ICollection<Order> Orders { get; set; }
}
