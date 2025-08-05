using Microsoft.EntityFrameworkCore;

namespace ShortenerService.Config;

public partial class ShortnerDbContext : DbContext
{
    private readonly ILogger<DbContext> _logger;
    public ShortnerDbContext()
    {
    }

    public ShortnerDbContext(DbContextOptions<ShortnerDbContext> options , ILogger<DbContext> logger) : base(options)
    {
        _logger = logger;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        _logger.LogInformation("PostgreSQL Database Connection Established");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new UrlConfiguration());
       
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
