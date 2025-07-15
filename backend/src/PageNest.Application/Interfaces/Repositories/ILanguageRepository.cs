using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface ILanguageRepository
{
    Task<IEnumerable<Language>> GetLanguages();
    Task<Language> GetLanguageById(Guid languageId);
}
