using Microsoft.EntityFrameworkCore;
namespace AuthService.Extensions;

public static class StorageServiceExtension
{
    public static void RegisterStorageService(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(x => x.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")
        ));
        var connection = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connection))
        {
            Console.WriteLine("No connection string found!");
        }
        else
        {
            Console.WriteLine("Connection string loaded from configuration.");
        }
    }

}