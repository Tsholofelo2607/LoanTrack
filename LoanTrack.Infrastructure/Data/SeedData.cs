using LoanTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanTrack.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if no products exist yet
        if (await context.LoanProducts.AnyAsync())
            return;

        var products = new List<LoanProduct>
        {
            new LoanProduct
            {
                Id = Guid.NewGuid(),
                Name = "Personal Loan",
                InterestRate = 15.5m,
                MinTermMonths = 6,
                MaxTermMonths = 60,
                MinAmount = 1000m,
                MaxAmount = 50000m,
                IsActive = true
            },
            new LoanProduct
            {
                Id = Guid.NewGuid(),
                Name = "Business Loan",
                InterestRate = 12.0m,
                MinTermMonths = 12,
                MaxTermMonths = 84,
                MinAmount = 10000m,
                MaxAmount = 500000m,
                IsActive = true
            },
            new LoanProduct
            {
                Id = Guid.NewGuid(),
                Name = "Student Loan",
                InterestRate = 8.5m,
                MinTermMonths = 12,
                MaxTermMonths = 120,
                MinAmount = 5000m,
                MaxAmount = 150000m,
                IsActive = true
            }
        };

        context.LoanProducts.AddRange(products);
        await context.SaveChangesAsync();
    }
}