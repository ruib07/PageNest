using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class LanguageRepository : ILanguageRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<Language> Languages => _context.Languages;

    public LanguageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Language>> GetLanguages()
    {
        return await Languages.AsNoTracking().ToListAsync();
    }

    public async Task<Language> GetLanguageById(Guid languageId)
    {
        return await Languages.AsNoTracking().FirstOrDefaultAsync(l => l.Id == languageId);
    }
}
