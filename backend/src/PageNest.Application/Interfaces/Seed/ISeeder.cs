﻿namespace PageNest.Application.Interfaces.Seed;

public interface ISeeder
{
    void SeedGenres();
    void SeedCategories();
    void SeedLanguages();
    void SeedAdmins();
}
