using Microsoft.EntityFrameworkCore;
using UrlTrimmer.WebApi.Models;

namespace UrlTrimmer.WebApi.Data;

public sealed class UrlShortenerDbContext : DbContext
{
    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).IsRequired().HasMaxLength(32);
            entity.Property(item => item.OriginalUrl).IsRequired().HasMaxLength(2048);
            entity.Property(item => item.CreatedUtc).IsRequired();
            entity.Property(item => item.ClickCount).IsRequired();
            entity.HasIndex(item => item.Code).IsUnique();
        });
    }
}