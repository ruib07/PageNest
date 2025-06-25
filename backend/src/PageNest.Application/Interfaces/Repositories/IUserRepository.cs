using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsers();
    Task<User> GetUserById(Guid userId);
    Task<User> GetUserByEmail(string email);
    Task<User> CreateUser(User user);
    Task<string> GeneratePasswordResetToken(Guid userId);
    Task<PasswordReset> GetPasswordResetToken(string token);
    Task RemovePasswordResetToken(PasswordReset token);
    Task UpdateUser(User user);
    Task DeleteUser(Guid userId);
}
