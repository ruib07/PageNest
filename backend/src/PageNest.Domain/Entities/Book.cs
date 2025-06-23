namespace PageNest.Domain.Entities;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public DateTime PublishedDate { get; set; }
    public string ISBN { get; set; }
    public int PageCount { get; set; }
    public string Language { get; set; }
    public string CoverImageUrl { get; set; }
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    public ICollection<BookGenre> BookGenres { get; set; }
    public ICollection<Review> Reviews { get; set; }
}
