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
            entity.Property(item => item.Code).IsRequired().HasMaxLength(32).HasColumnName("code");
            entity.Property(item => item.OriginalUrl).IsRequired().HasMaxLength(2048).HasColumnName("original_url");
            entity.Property(item => item.ClerkUserId).IsRequired().HasMaxLength(128).HasColumnName("clerk_user_id");
            entity.Property(item => item.CreatedAt).IsRequired().HasColumnType("timestamptz").HasColumnName("created_at");
            entity.Property(item => item.UpdatedAt).IsRequired().HasColumnType("timestamptz").HasColumnName("updated_at");
            entity.HasIndex(item => item.Code).IsUnique().HasDatabaseName("idx_short_urls_code");
        });
    }
}