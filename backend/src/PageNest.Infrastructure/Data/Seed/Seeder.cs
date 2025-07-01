using Microsoft.Extensions.Configuration;
using PageNest.Application.Constants;
using PageNest.Application.Helpers;
using PageNest.Application.Interfaces.Seed;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Seed;

public class Seeder : ISeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public Seeder(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public void SeedGenres()
    {
        if (_context.Genres.Any()) return;

        var seedGenres = _configuration.GetSection(AppSettings.GenreSeedSection).Get<List<SeederDTO.GenreSeederDTO>>();

        if (seedGenres == null || !seedGenres.Any()) return;

        var genres = seedGenres.Select(g => new Genre()
        {
            Id = g.Id,
            Name = g.Name
        });

        _context.Genres.AddRange(genres);
        _context.SaveChanges();
    }

    public void SeedCategories()
    {
        if (_context.Categories.Any()) return;

        var seedCategories = _configuration.GetSection(AppSettings.CategorySeedSection).Get<List<SeederDTO.CategorySeederDTO>>();

        if (seedCategories == null || !seedCategories.Any()) return;

        var categories = seedCategories.Select(c => new Category()
        {
            Id = c.Id,
            Name = c.Name
        });

        _context.Categories.AddRange(categories);
        _context.SaveChanges();
    }

    public void SeedLanguages()
    {
        if (_context.Languages.Any()) return;

        var seedLanguages = _configuration.GetSection(AppSettings.LanguageSeedSection).Get<List<SeederDTO.LanguageSeederDTO>>();

        if (seedLanguages == null || !seedLanguages.Any()) return;

        var languages = seedLanguages.Select(l => new Language()
        {
            Id = l.Id,
            Name = l.Name,
            Code = l.Code,
            CultureCode = l.CultureCode
        });

        _context.Languages.AddRange(languages);
        _context.SaveChanges();
    }

    public void SeedAdmins()
    {
        if (_context.Users.Where(u => u.Role == Roles.Admin).Any()) return;

        var seedAdmin = _configuration.GetSection(AppSettings.AdminSeedSection).Get<SeederDTO.AdminSeederDTO>();

        if (seedAdmin == null) return;

        var admin = new User()
        {
            Id = seedAdmin.Id,
            Name = seedAdmin.Name,
            Email = seedAdmin.Email,
            PasswordHash = PasswordHasherHelper.HashPassword(seedAdmin.Password),
            Role = Roles.Admin
        };

        _context.Users.Add(admin);
        _context.SaveChanges();
    }
}
