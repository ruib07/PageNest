using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<User> Users => _context.Users;
    private DbSet<PasswordReset> PasswordResets => _context.PasswordResets;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        return await Users.AsNoTracking().ToListAsync();
    }

    public async Task<User> GetUserById(Guid userId)
    {
        return await Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> GetUserByEmail(string email)
    {
        return await Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateUser(User user)
    {
        await Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<PasswordReset> GetPasswordResetToken(string token)
    {
        return await PasswordResets.Include(pr => pr.User)
                                   .FirstOrDefaultAsync(pr => pr.Token == token && pr.ExpirationDate > DateTime.UtcNow);
    }

    public async Task<string> GeneratePasswordResetToken(Guid userId)
    {
        var passwordReset = new PasswordReset()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString(),
            ExpirationDate = DateTime.UtcNow.AddHours(1)
        };

        await PasswordResets.AddAsync(passwordReset);
        await _context.SaveChangesAsync();

        return passwordReset.Token;
    }

    public async Task RemovePasswordResetToken(PasswordReset token)
    {
        PasswordResets.Remove(token);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUser(User user)
    {
        Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUser(Guid userId)
    {
        var user = await GetUserById(userId);

        Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
