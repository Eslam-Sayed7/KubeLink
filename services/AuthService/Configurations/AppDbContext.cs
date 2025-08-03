using AuthService.Configurations;
using AuthService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService;

public partial class AppDbContext : IdentityDbContext<AppUser>
{
    private readonly ILogger<DbContext> _logger;
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options , ILogger<DbContext> logger) : base(options)
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
        
        modelBuilder.ApplyConfiguration(new AppUserConfiguration());

         // Seed Roles
         modelBuilder.Entity<IdentityRole>().HasData(
             new IdentityRole { Id = "1" , Name = "Admin", NormalizedName = "ADMIN" },
             new IdentityRole { Id = "2" , Name = "User", NormalizedName = "USER" }
         );
       
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
