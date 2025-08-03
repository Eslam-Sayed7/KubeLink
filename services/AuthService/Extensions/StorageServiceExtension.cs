using Microsoft.EntityFrameworkCore;
namespace AuthService.Extensions;

public static class StorageServiceExtension
{
    public static void RegisterStorageService(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(x => x.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")));
    }

}