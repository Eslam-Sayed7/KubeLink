using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShortenerService.Entities;

namespace ShortenerService.Config;

public class UrlConfiguration : IEntityTypeConfiguration<URL>
{
    public void Configure(EntityTypeBuilder<URL> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.UserId).IsRequired().HasMaxLength(36);
        builder.Property(u => u.OriginalUrl).IsRequired().HasMaxLength(2048); 
        builder.Property(u => u.ShortenedUrl).IsRequired().HasMaxLength(100); 
        builder.Property(u => u.CreatedAt).IsRequired().HasColumnType("timestamp");
        builder.Property(u => u.AccessCount).IsRequired().HasDefaultValue(0);
        builder.Property(u => u.ExpirationDate).HasColumnType("timestamp");
        
        // Indexes
        builder.HasIndex(u => u.ShortenedUrl).IsUnique();
    }
    public void Configure(EntityTypeBuilder<URL> builder, string schema)
    {
        builder.ToTable("urls", schema);
        Configure(builder);
    }
}