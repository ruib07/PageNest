namespace PageNest.Domain.Entities;

public class Language
{
    public Guid Id { get; set; }
    public string Name { get; set; } // ex: "Portuguese"
    public string Code { get; set; } // ex: "PT"
    public string CultureCode { get; set; } // ex: "pt-PT"
    public ICollection<Book> Books { get; set; }
}
