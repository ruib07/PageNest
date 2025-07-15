using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class LanguagesService : ILanguagesService
{
    private readonly ILanguageRepository _languageRepository;

    public LanguagesService(ILanguageRepository languageRepository)
    {
        _languageRepository = languageRepository;
    }

    public async Task<IEnumerable<Language>> GetLanguages()
    {
        return await _languageRepository.GetLanguages();
    }

    public async Task<Result<Language>> GetLanguageById(Guid languageId)
    {
        var language = await _languageRepository.GetLanguageById(languageId);

        if (language == null) return Result<Language>.Fail("Language not found.", 404);

        return Result<Language>.Success(language);
    }
}
