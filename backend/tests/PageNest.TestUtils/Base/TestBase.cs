using PageNest.Infrastructure.Data.Context;

namespace PageNest.TestUtils.Base;

public class TestBase : IDisposable
{
    protected readonly ApplicationDbContext _context;

    protected TestBase(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
