using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface ILanguagesService
{
    Task<IEnumerable<Language>> GetLanguages();
    Task<Result<Language>> GetLanguageById(Guid languageId);
}
