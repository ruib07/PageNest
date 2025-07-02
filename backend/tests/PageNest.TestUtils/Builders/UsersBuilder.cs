using PageNest.Application.Helpers;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;

namespace PageNest.TestUtils.Builders;

public class UsersBuilder
{
    private static int _counter = 2;

    public static List<User> CreateUsers(int quantity = 2)
    {
        var users = new List<User>();

        for (int i = 0; i < quantity; i++)
        {
            users.Add(new User()
            {
                Id = Guid.NewGuid(),
                Name = $"User {_counter}",
                Email = $"user{_counter}@email.com",
                PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
                Role = Roles.User
            });

            _counter++;
        }

        return users;
    }

    public static User CreateAdmin()
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Name = "User Name",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.Admin
        };
    }

    public static User InvalidUserCreation(string name, string email, string password)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = password,
            Role = Roles.User
        };
    }

    public static PasswordResetDTO.UpdatePassword UpdatePasswordCreation(string token, string newPassword, string confirmNewPassword)
    {
        return new PasswordResetDTO.UpdatePassword(token, newPassword, confirmNewPassword);
    }

    public static PasswordReset CreateToken(string token, User user)
    {
        return new PasswordReset()
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpirationDate = DateTime.UtcNow,
            UserId = user.Id,
            User = user
        };
    }

    public static User UpdateUser(Guid id)
    {
        return new User()
        {
            Id = id,
            Name = "User Update",
            Email = "userupdated@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Update-123"),
            Role = Roles.User
        };
    }
}
