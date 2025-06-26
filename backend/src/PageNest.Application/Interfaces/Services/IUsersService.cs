using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IUsersService
{
    Task<IEnumerable<User>> GetUsers();
    Task<Result<User>> GetUserById(Guid userId);
    Task<Result<User>> CreateUser(User user);
    Task<string> GeneratePasswordResetToken(string email);
    Task<Result<User>> UpdatePassword(string token, string newPassword, string confirmNewPassword);
    Task<Result<User>> UpdateUser(Guid userId, User updateUser);
    Task DeleteUser(Guid userId);
}
