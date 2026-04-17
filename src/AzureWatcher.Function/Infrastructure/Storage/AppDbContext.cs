using AzureWatcher.Function.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AzureWatcher.Function.Infrastructure.Storage;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<JobOffer> ProcessedJobOffers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<JobOffer>().HasKey(x => x.Id);
    }
}
