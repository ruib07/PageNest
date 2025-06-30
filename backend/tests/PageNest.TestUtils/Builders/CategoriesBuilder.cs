using PageNest.Domain.Entities;

namespace PageNest.TestUtils.Builders;

public class CategoriesBuilder
{
    private static int _counter = 2;

    public static List<Category> CreateCategories(int quantity = 2)
    {
        var categories = new List<Category>();

        for (int i = 0; i < quantity; i++)
        {
            categories.Add(new Category()
            {
                Id = Guid.NewGuid(),
                Name = $"Category {_counter}"
            });

            _counter++;
        }

        return categories;
    }
}
