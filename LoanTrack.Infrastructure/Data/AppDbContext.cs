using LoanTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanTrack.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LoanProduct> LoanProducts => Set<LoanProduct>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<RepaymentSchedule> RepaymentSchedules => Set<RepaymentSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique(); // no duplicate emails
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        // LoanProduct
        modelBuilder.Entity<LoanProduct>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.InterestRate).HasPrecision(5, 2);  // e.g. 15.50
            entity.Property(p => p.MinAmount).HasPrecision(18, 2);
            entity.Property(p => p.MaxAmount).HasPrecision(18, 2);
        });

        // LoanApplication
        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.RequestedAmount).HasPrecision(18, 2);
            entity.Property(a => a.Purpose).IsRequired().HasMaxLength(500);

            // An applicant (User) has many LoanApplications
            entity.HasOne(a => a.Applicant)
                  .WithMany(u => u.LoanApplications)
                  .HasForeignKey(a => a.ApplicantId)
                  .OnDelete(DeleteBehavior.Restrict);

            // A LoanApplication belongs to one LoanProduct
            entity.HasOne(a => a.LoanProduct)
                  .WithMany(p => p.LoanApplications)
                  .HasForeignKey(a => a.LoanProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // The reviewing officer (User) — optional relationship
            entity.HasOne(a => a.ReviewedByUser)
                  .WithMany()
                  .HasForeignKey(a => a.ReviewedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // RepaymentSchedule
        modelBuilder.Entity<RepaymentSchedule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.AmountDue).HasPrecision(18, 2);
            entity.Property(r => r.AmountPaid).HasPrecision(18, 2);

            entity.HasOne(r => r.LoanApplication)
                  .WithMany(a => a.RepaymentSchedules)
                  .HasForeignKey(r => r.LoanApplicationId)
                  .OnDelete(DeleteBehavior.Cascade); // delete schedules if loan is deleted
        });
    }
}