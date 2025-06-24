using Microsoft.Extensions.Configuration;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
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
}
